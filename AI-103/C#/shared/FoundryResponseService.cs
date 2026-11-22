#pragma warning disable OPENAI001
using System.Runtime.CompilerServices;
using OpenAI.Responses;

namespace AzureExamsTraining.AI103.Shared;

public sealed record ResponseAnswer(
    string Text,
    string ResponseId);
public sealed record StreamChunk(
    string Text,
    string? ResponseId,
    bool Completed);
/// <summary>Minimal wrapper for the Responses API with optional response-id chaining.</summary>
public sealed class FoundryResponseService
{
    private readonly ResponsesClient _responsesClient;
    private readonly FoundryOptions _foundryOptions;
    public FoundryResponseService(FoundryOptions settings)
    {
        _foundryOptions = settings;
        _responsesClient = FoundryClientFactory.CreateResponsesClient(settings);
    }

    private CreateResponseOptions MakeOptions(
        string messageString,
        string? previousIdString,
        bool streamBool = false)
    {
        if (string.IsNullOrWhiteSpace(messageString))
        {
            throw new ArgumentException(
                "Enter a non-empty message.",
                nameof(messageString));
        }

        var createResponseOptions = new CreateResponseOptions
        {
            Model = _foundryOptions.ModelDeployment,
            Instructions = _foundryOptions.Instructions,
            PreviousResponseId = previousIdString,
            StreamingEnabled = streamBool
        };
        createResponseOptions.InputItems.Add(ResponseItem.CreateUserMessageItem(messageString));
        return createResponseOptions;
    }

    public async Task<ResponseAnswer> SendAsync(
        string message,
        string? previousId = null,
        CancellationToken cancellationToken = default)
    {
        ResponseResult resultResponseResult = await _responsesClient.CreateResponseAsync(
            MakeOptions(
                message,
                previousId),
            cancellationToken);
        return new ResponseAnswer(
            resultResponseResult.GetOutputText(),
            resultResponseResult.Id);
    }

    public async IAsyncEnumerable<StreamChunk> StreamAsync(
        string message,
        string? previousId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool finishedBool = false;
        await foreach (StreamingResponseUpdate updateStreamingResponseUpdate in await _responsesClient.CreateResponseStreamingAsync(
            MakeOptions(
                message,
                previousId,
                streamBool: true),
            cancellationToken))
        {
            if (updateStreamingResponseUpdate is StreamingResponseOutputTextDeltaUpdate deltaStreamingResponseOutputTextDeltaUpdate)
            {
                yield return new StreamChunk(
                    deltaStreamingResponseOutputTextDeltaUpdate.Delta,
                    null,
                    false);
            }
            else if (updateStreamingResponseUpdate is StreamingResponseCompletedUpdate completedStreamingResponseCompletedUpdate)
            {
                finishedBool = true;
                yield return new StreamChunk(
                    string.Empty,
                    completedStreamingResponseCompletedUpdate.Response.Id,
                    true);
            }
        }

        if (!finishedBool)
        {
            throw new InvalidOperationException("The response stream closed without a completion event.");
        }
    }
}
