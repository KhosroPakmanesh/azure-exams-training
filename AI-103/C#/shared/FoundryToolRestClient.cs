using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AzureExamsTraining.AI103.Shared;
/// <summary>
/// Small REST adapter for the Azure OpenAI /openai/v1/ Responses, Files and
/// Vector Stores endpoints. The older chat labs continue to use the OpenAI SDK.
/// Built-in tool request shapes are explicit here so the five lessons mirror
/// the Microsoft Learn REST payloads without requiring another NuGet package.
/// </summary>
public sealed class FoundryToolRestClient : IDisposable
{
    private readonly FoundryOptions _foundryOptions;
    private readonly HttpClient _httpHttpClient;
    private readonly DefaultAzureCredential? _credentialDefaultAzureCredential;
    public FoundryToolRestClient(FoundryOptions settings)
    {
        _foundryOptions = settings;
        _httpHttpClient = new HttpClient
        {
            BaseAddress = settings.Endpoint,
            Timeout = TimeSpan.FromMinutes(5)
        };
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            _credentialDefaultAzureCredential = new DefaultAzureCredential();
        }
    }

    public Task<JsonDocument> CreateResponseAsync(
        object input,
        object[] tools,
        string? previousResponseId = null,
        string? instructions = null,
        string[]? include = null,
        CancellationToken cancellationToken = default)
    {
        var payloadStringObjectDictionary = new Dictionary<string, object?>
        {
            ["model"] = _foundryOptions.ModelDeployment,
            ["instructions"] = instructions ?? _foundryOptions.Instructions,
            ["input"] = input,
            ["tools"] = tools,
            ["store"] = true
        };
        if (!string.IsNullOrWhiteSpace(previousResponseId))
        {
            payloadStringObjectDictionary["previous_response_id"] = previousResponseId;
        }

        if (include is { Length: > 0 })
        {
            payloadStringObjectDictionary["include"] = include;
        }

        return PostJsonAsync(
            "responses",
            payloadStringObjectDictionary,
            cancellationToken);
    }

    public Task<JsonDocument> PostJsonAsync(
        string relativePath,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Post,
            relativePath,
            new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"),
            cancellationToken);
    }

    public Task<JsonDocument> GetAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            cancellationToken);
    }

    public async Task<JsonDocument> UploadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        using var formMultipartFormDataContent = new MultipartFormDataContent();
        formMultipartFormDataContent.Add(
            new StringContent("assistants"),
            "purpose");
        await using FileStream? fileFileStream = File.OpenRead(filePath);
        using var fileContentStreamContent = new StreamContent(fileFileStream);
        fileContentStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formMultipartFormDataContent.Add(
            fileContentStreamContent,
            "file",
            Path.GetFileName(filePath));
        // The multipart content is disposed after the request has finished.
        return await SendAsync(
            HttpMethod.Post,
            "files",
            formMultipartFormDataContent,
            cancellationToken);
    }

    private async Task<JsonDocument> SendAsync(
        HttpMethod methodHttpMethod,
        string relativePathString,
        HttpContent? bodyHttpContent,
        CancellationToken cancellationToken)
    {
        using var requestHttpRequestMessage = new HttpRequestMessage(
            methodHttpMethod,
            relativePathString)
        {
            Content = bodyHttpContent
        };
        if (_credentialDefaultAzureCredential is null)
        {
            requestHttpRequestMessage.Headers.Add(
                "api-key",
                _foundryOptions.ApiKey);
        }
        else
        {
            // Azure OpenAI v1 resource endpoints accept Microsoft Entra tokens.
            AccessToken tokenAccessToken = await _credentialDefaultAzureCredential.GetTokenAsync(
                new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]),
                cancellationToken);
            requestHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tokenAccessToken.Token);
        }

        using HttpResponseMessage? responseHttpResponseMessage = await _httpHttpClient.SendAsync(
            requestHttpRequestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        string? contentString = await responseHttpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (!responseHttpResponseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Foundry returned HTTP {(int)responseHttpResponseMessage.StatusCode} ({responseHttpResponseMessage.ReasonPhrase}): " + contentString[..Math.Min(
                    contentString.Length,
                    1800)]);
        }

        return JsonDocument.Parse(contentString);
    }

    public static string RequireId(JsonDocument response)
    {
        return response.RootElement.TryGetProperty(
            "id",
            out JsonElement idJsonElement) && idJsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(idJsonElement.GetString()) ? idJsonElement.GetString()! : throw new InvalidOperationException("The service response did not include an ID.");
    }

    public static string GetOutputText(JsonDocument response)
    {
        var textStringBuilder = new StringBuilder();
        if (response.RootElement.TryGetProperty(
            "output",
            out JsonElement outputJsonElement) && outputJsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement itemJsonElement in outputJsonElement.EnumerateArray())
            {
                if (!itemJsonElement.TryGetProperty(
                    "content",
                    out JsonElement contentJsonElement) || contentJsonElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (JsonElement partJsonElement in contentJsonElement.EnumerateArray())
                {
                    if (partJsonElement.TryGetProperty(
                        "type",
                        out JsonElement typeJsonElement) && typeJsonElement.GetString() == "output_text" && partJsonElement.TryGetProperty(
                            "text",
                            out JsonElement valueJsonElement))
                    {
                        textStringBuilder.Append(valueJsonElement.GetString());
                    }
                }
            }
        }

        return textStringBuilder.Length > 0 ? textStringBuilder.ToString() : "(No assistant text was returned.)";
    }

    public static void PrintAnswer(JsonDocument response)
    {
        Console.WriteLine("Assistant: " + GetOutputText(response));
        // Show source metadata returned as file or URL annotations when available.
        var citationStrings = new HashSet<string>(StringComparer.Ordinal);
        if (response.RootElement.TryGetProperty(
            "output",
            out JsonElement outputJsonElement) && outputJsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement itemJsonElement in outputJsonElement.EnumerateArray())
            {
                if (!itemJsonElement.TryGetProperty(
                    "content",
                    out JsonElement contentJsonElement) || contentJsonElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (JsonElement partJsonElement in contentJsonElement.EnumerateArray())
                {
                    if (!partJsonElement.TryGetProperty(
                        "annotations",
                        out JsonElement annotationsJsonElement) || annotationsJsonElement.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (JsonElement annotationJsonElement in annotationsJsonElement.EnumerateArray())
                    {
                        if (!annotationJsonElement.TryGetProperty(
                            "type",
                            out JsonElement typeJsonElement))
                        {
                            continue;
                        }

                        string? citationString = typeJsonElement.GetString() switch
                        {
                            "url_citation" when annotationJsonElement.TryGetProperty(
                                "url",
                                out JsonElement urlJsonElement) => urlJsonElement.GetString(),
                            "file_citation" when annotationJsonElement.TryGetProperty(
                                "filename",
                                out JsonElement filenameJsonElement) => "File: " + filenameJsonElement.GetString(),
                            _ => null
                        };
                        if (!string.IsNullOrWhiteSpace(citationString))
                        {
                            citationStrings.Add(citationString);
                        }
                    }
                }
            }
        }

        foreach (string? citationString in citationStrings)
        {
            Console.WriteLine("  Source: " + citationString);
        }

        Console.WriteLine();
    }

    public void Dispose()
    {
        _httpHttpClient.Dispose();
    }
}
