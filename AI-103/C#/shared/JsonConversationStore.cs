using System.Text.Json;

namespace AzureExamsTraining.AI103.Shared;
/// <summary>Simple local JSON persistence for learning. Not a production multi-node database.</summary>
public sealed class JsonConversationStore
{
    private static readonly JsonSerializerOptions JsonJsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly string _directoryString;
    private readonly string? _legacyDirectoryString;

    public JsonConversationStore(string? directory = null)
    {
        string? configuredDirectoryString = directory ?? Environment.GetEnvironmentVariable("CHAT_HISTORY_DIR");
        if (configuredDirectoryString is not null)
        {
            _directoryString = configuredDirectoryString;
        }
        else
        {
            string appDataDirectoryString = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _directoryString = Path.Combine(
                appDataDirectoryString,
                "AzureExamsTraining",
                "AI-103",
                "CSharp",
                "Conversations");
            _legacyDirectoryString = Path.Combine(
                appDataDirectoryString,
                "FoundryCSharpLabs",
                "Conversations");
        }

        Directory.CreateDirectory(_directoryString);
    }

    public async Task<ConversationRecord> CreateAsync(
        string? title = null,
        CancellationToken ct = default)
    {
        ConversationRecord conversationConversationRecord = new();
        if (!string.IsNullOrWhiteSpace(title))
        {
            conversationConversationRecord.Title = title.Trim()[..Math.Min(
                80,
                title.Trim().Length)];
        }

        await SaveAsync(
            conversationConversationRecord,
            ct);
        return conversationConversationRecord;
    }

    public async Task<ConversationRecord?> GetAsync(
        Guid id,
        CancellationToken ct = default)
    {
        foreach (string directoryString in ReadDirectories())
        {
            string fileString = FilePath(
                directoryString,
                id);
            if (File.Exists(fileString))
            {
                return await ReadAsync(
                    fileString,
                    ct);
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<ConversationSummary>> ListAsync(CancellationToken ct = default)
    {
        List<ConversationSummary> conversationSummaries = [];
        HashSet<string> fileNameStrings = new(StringComparer.OrdinalIgnoreCase);
        foreach (string directoryString in ReadDirectories())
        {
            foreach (string fileString in Directory.EnumerateFiles(
                directoryString,
                "*.json"))
            {
                ct.ThrowIfCancellationRequested();
                if (!fileNameStrings.Add(Path.GetFileName(fileString)))
                {
                    continue;
                }

                ConversationRecord conversationRecord = await ReadAsync(
                    fileString,
                    ct);
                conversationSummaries.Add(
                    new ConversationSummary(
                        conversationRecord.Id,
                        conversationRecord.Title,
                        conversationRecord.UpdatedAtUtc,
                        conversationRecord.Messages.Count));
            }
        }

        return conversationSummaries.OrderByDescending(conversationSummary => conversationSummary.UpdatedAtUtc).ToArray();
    }

    public async Task SaveAsync(
        ConversationRecord conversation,
        CancellationToken ct = default)
    {
        conversation.UpdatedAtUtc = DateTimeOffset.UtcNow;
        string fileString = FilePath(
            _directoryString,
            conversation.Id);
        string? temporaryString = fileString + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var outputFileStream = new FileStream(
                temporaryString,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    outputFileStream,
                    conversation,
                    JsonJsonSerializerOptions,
                    ct);
                await outputFileStream.FlushAsync(ct);
            }

            File.Move(
                temporaryString,
                fileString,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryString))
            {
                File.Delete(temporaryString);
            }
        }
    }

    public async Task AppendExchangeAsync(
        ConversationRecord conversation,
        string userText,
        ResponseAnswer answer,
        CancellationToken ct = default)
    {
        DateTimeOffset nowDateTimeOffset = DateTimeOffset.UtcNow;
        conversation.Messages.Add(new ChatTurn(
            "user",
            userText,
            nowDateTimeOffset));
        conversation.Messages.Add(new ChatTurn(
            "assistant",
            answer.Text,
            DateTimeOffset.UtcNow));
        conversation.LastResponseId = answer.ResponseId;
        await SaveAsync(
            conversation,
            ct);
    }

    private IEnumerable<string> ReadDirectories()
    {
        // Histories saved in the new directory take precedence over the previous defaults.
        yield return _directoryString;
        if (_legacyDirectoryString is not null && Directory.Exists(_legacyDirectoryString))
        {
            yield return _legacyDirectoryString;
        }
    }

    private static string FilePath(
        string directoryString,
        Guid idGuid)
    {
        return Path.Combine(
            directoryString,
            $"{idGuid:D}.json");
    }

    private static async Task<ConversationRecord> ReadAsync(
        string fileString,
        CancellationToken cancellationToken)
    {
        await using FileStream inputFileStream = File.OpenRead(fileString);
        return await JsonSerializer.DeserializeAsync<ConversationRecord>(
            inputFileStream,
            JsonJsonSerializerOptions,
            cancellationToken) ?? throw new InvalidDataException($"Empty conversation file: {fileString}");
    }
}
