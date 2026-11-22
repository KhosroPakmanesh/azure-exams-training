using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

namespace AzureExamsTraining.AI103.FineTuning;
/// <summary>
/// Separate uploader because FoundryToolRestClient.UploadFileAsync intentionally uses
/// purpose=assistants for File Search, while fine-tuning requires purpose=fine-tune.
/// </summary>
internal sealed class TrainingFileUploader : IDisposable
{
    private readonly FoundryOptions _foundryOptions;
    private readonly HttpClient _httpHttpClient;
    internal TrainingFileUploader(FoundryOptions foundryOptions)
    {
        _foundryOptions = foundryOptions;
        _httpHttpClient = new HttpClient
        {
            BaseAddress = foundryOptions.Endpoint,
            Timeout = TimeSpan.FromMinutes(5)
        };
    }

    internal async Task<JsonDocument> UploadAsync(string pathString)
    {
        using var multipartMultipartFormDataContent = new MultipartFormDataContent();
        multipartMultipartFormDataContent.Add(
            new StringContent("fine-tune"),
            "purpose");
        await using FileStream? fileFileStream = File.OpenRead(pathString);
        using var contentStreamContent = new StreamContent(fileFileStream);
        contentStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipartMultipartFormDataContent.Add(
            contentStreamContent,
            "file",
            Path.GetFileName(pathString));
        using var requestHttpRequestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "files")
        {
            Content = multipartMultipartFormDataContent
        };
        if (!string.IsNullOrWhiteSpace(_foundryOptions.ApiKey))
        {
            requestHttpRequestMessage.Headers.Add(
                "api-key",
                _foundryOptions.ApiKey);
        }
        else
        {
            AccessToken tokenAccessToken = await new DefaultAzureCredential().GetTokenAsync(new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]));
            requestHttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tokenAccessToken.Token);
        }

        using HttpResponseMessage? responseHttpResponseMessage = await _httpHttpClient.SendAsync(requestHttpRequestMessage);
        string bodyString = await responseHttpResponseMessage.Content.ReadAsStringAsync();
        if (!responseHttpResponseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Upload failed HTTP {(int)responseHttpResponseMessage.StatusCode}: {bodyString[..Math.Min(
                    bodyString.Length,
                    1000)]}");
        }

        return JsonDocument.Parse(bodyString);
    }

    public void Dispose()
    {
        _httpHttpClient.Dispose();
    }
}
