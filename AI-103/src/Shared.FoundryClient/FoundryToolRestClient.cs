using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FoundryLabs.Shared;

/// <summary>
/// Small REST adapter for the Azure OpenAI /openai/v1/ Responses, Files and
/// Vector Stores endpoints. The older chat labs continue to use the OpenAI SDK.
/// Built-in tool request shapes are explicit here so the five lessons mirror
/// the Microsoft Learn REST payloads without requiring another NuGet package.
/// </summary>
public sealed class FoundryToolRestClient : IDisposable
{
    private readonly FoundryOptions _settings;
    private readonly HttpClient _http;
    private readonly DefaultAzureCredential? _credential;

    public FoundryToolRestClient(FoundryOptions settings)
    {
        _settings = settings;
        _http = new HttpClient { BaseAddress = settings.Endpoint, Timeout = TimeSpan.FromMinutes(5) };
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            _credential = new DefaultAzureCredential();
    }

    public Task<JsonDocument> CreateResponseAsync(
        object input,
        object[] tools,
        string? previousResponseId = null,
        string? instructions = null,
        string[]? include = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = _settings.ModelDeployment,
            ["instructions"] = instructions ?? _settings.Instructions,
            ["input"] = input,
            ["tools"] = tools,
            ["store"] = true
        };
        if (!string.IsNullOrWhiteSpace(previousResponseId))
            payload["previous_response_id"] = previousResponseId;
        if (include is { Length: > 0 })
            payload["include"] = include;
        return PostJsonAsync("responses", payload, cancellationToken);
    }

    public Task<JsonDocument> PostJsonAsync(string relativePath, object payload,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, relativePath,
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

    public Task<JsonDocument> GetAsync(string relativePath,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, relativePath, null, cancellationToken);

    public async Task<JsonDocument> UploadFileAsync(string filePath,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("assistants"), "purpose");
        await using var file = File.OpenRead(filePath);
        using var fileContent = new StreamContent(file);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", Path.GetFileName(filePath));
        // The multipart content is disposed after the request has finished.
        return await SendAsync(HttpMethod.Post, "files", form, cancellationToken);
    }

    private async Task<JsonDocument> SendAsync(HttpMethod method, string relativePath,
        HttpContent? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativePath) { Content = body };
        if (_credential is null)
        {
            request.Headers.Add("api-key", _settings.ApiKey);
        }
        else
        {
            // Azure OpenAI v1 resource endpoints accept Microsoft Entra tokens.
            var token = await _credential.GetTokenAsync(
                new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]),
                cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }
        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Foundry returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): " +
                content[..Math.Min(content.Length, 1800)]);
        return JsonDocument.Parse(content);
    }

    public static string RequireId(JsonDocument response) =>
        response.RootElement.TryGetProperty("id", out var id) &&
        id.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(id.GetString())
            ? id.GetString()!
            : throw new InvalidOperationException("The service response did not include an ID.");

    public static string GetOutputText(JsonDocument response)
    {
        var text = new StringBuilder();
        if (response.RootElement.TryGetProperty("output", out var output) &&
            output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content) ||
                    content.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var part in content.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var type) &&
                        type.GetString() == "output_text" &&
                        part.TryGetProperty("text", out var value))
                    {
                        text.Append(value.GetString());
                    }
                }
            }
        }
        return text.Length > 0 ? text.ToString() : "(No assistant text was returned.)";
    }

    public static void PrintAnswer(JsonDocument response)
    {
        Console.WriteLine("Assistant: " + GetOutputText(response));

        // Show source metadata returned as file or URL annotations when available.
        var citations = new HashSet<string>(StringComparer.Ordinal);
        if (response.RootElement.TryGetProperty("output", out var output) &&
            output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content) ||
                    content.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var part in content.EnumerateArray())
                {
                    if (!part.TryGetProperty("annotations", out var annotations) ||
                        annotations.ValueKind != JsonValueKind.Array)
                        continue;
                    foreach (var annotation in annotations.EnumerateArray())
                    {
                        if (!annotation.TryGetProperty("type", out var type)) continue;
                        string? citation = type.GetString() switch
                        {
                            "url_citation" when annotation.TryGetProperty("url", out var url) =>
                                url.GetString(),
                            "file_citation" when annotation.TryGetProperty("filename", out var filename) =>
                                "File: " + filename.GetString(),
                            _ => null
                        };
                        if (!string.IsNullOrWhiteSpace(citation)) citations.Add(citation);
                    }
                }
            }
        }
        foreach (var citation in citations)
            Console.WriteLine("  Source: " + citation);
        Console.WriteLine();
    }

    public void Dispose() => _http.Dispose();
}
