#pragma warning disable OPENAI001
using System.Runtime.CompilerServices;
using OpenAI.Responses;

namespace FoundryLabs.Shared;

public sealed record ResponseAnswer(string Text, string ResponseId);
public sealed record StreamChunk(string Text, string? ResponseId, bool Completed);

/// <summary>Minimal wrapper for the Responses API with optional response-id chaining.</summary>
public sealed class FoundryResponseService
{
    private readonly ResponsesClient _client;
    private readonly FoundryOptions _settings;

    public FoundryResponseService(FoundryOptions settings)
    {
        _settings = settings;
        _client = FoundryClientFactory.CreateResponsesClient(settings);
    }

    private CreateResponseOptions MakeOptions(string message, string? previousId, bool stream = false)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Enter a non-empty message.", nameof(message));
        var options = new CreateResponseOptions
        {
            Model = _settings.ModelDeployment,
            Instructions = _settings.Instructions,
            PreviousResponseId = previousId,
            StreamingEnabled = stream
        };
        options.InputItems.Add(ResponseItem.CreateUserMessageItem(message));
        return options;
    }

    public async Task<ResponseAnswer> SendAsync(string message, string? previousId = null,
        CancellationToken cancellationToken = default)
    {
        ResponseResult result = await _client.CreateResponseAsync(
            MakeOptions(message, previousId), cancellationToken);
        return new ResponseAnswer(result.GetOutputText(), result.Id);
    }

    public async IAsyncEnumerable<StreamChunk> StreamAsync(string message, string? previousId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool finished = false;
        await foreach (StreamingResponseUpdate update
            in await _client.CreateResponseStreamingAsync(
                MakeOptions(message, previousId, stream: true),
                cancellationToken))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate delta)
                yield return new StreamChunk(delta.Delta, null, false);
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                finished = true;
                yield return new StreamChunk(string.Empty, completed.Response.Id, true);
            }
        }
        if (!finished)
            throw new InvalidOperationException("The response stream closed without a completion event.");
    }
}
