#pragma warning disable OPENAI001 // The Responses SDK surface is currently marked experimental.

using Azure.Identity;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AzureExamsTraining.AI103.Shared;

public static class FoundryClientFactory
{
    public static ChatClient CreateChatClient(FoundryOptions settings)
    {
        OpenAIClientOptions openAIClientOptions = new()
        {
            Endpoint = settings.Endpoint
        };
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return new ChatClient(
                settings.ModelDeployment,
                new ApiKeyCredential(settings.ApiKey),
                openAIClientOptions);
        }

        return new ChatClient(
            settings.ModelDeployment,
            TokenPolicy(),
            openAIClientOptions);
    }

    public static ResponsesClient CreateResponsesClient(FoundryOptions settings)
    {
        ResponsesClientOptions responsesClientOptions = new()
        {
            Endpoint = settings.Endpoint
        };
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return new ResponsesClient(
                new ApiKeyCredential(settings.ApiKey),
                responsesClientOptions);
        }

        return new ResponsesClient(
            TokenPolicy(),
            responsesClientOptions);
    }

    private static BearerTokenPolicy TokenPolicy()
    {
        return new(
            new DefaultAzureCredential(),
            "https://ai.azure.com/.default");
    }
}
