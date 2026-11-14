#pragma warning disable OPENAI001 // The Responses SDK surface is currently marked experimental.
using Azure.Identity;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace FoundryLabs.Shared;

public static class FoundryClientFactory
{
    public static ChatClient CreateChatClient(FoundryOptions settings)
    {
        OpenAIClientOptions options = new() { Endpoint = settings.Endpoint };
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            return new ChatClient(settings.ModelDeployment, new ApiKeyCredential(settings.ApiKey), options);
        return new ChatClient(settings.ModelDeployment, TokenPolicy(), options);
    }

    public static ResponsesClient CreateResponsesClient(FoundryOptions settings)
    {
        ResponsesClientOptions options = new() { Endpoint = settings.Endpoint };
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            return new ResponsesClient(new ApiKeyCredential(settings.ApiKey), options);
        return new ResponsesClient(TokenPolicy(), options);
    }

    private static BearerTokenPolicy TokenPolicy() =>
        new(new DefaultAzureCredential(), "https://ai.azure.com/.default");
}
