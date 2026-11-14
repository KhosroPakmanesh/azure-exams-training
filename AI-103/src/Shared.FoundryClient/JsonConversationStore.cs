using System.Text.Json;

namespace FoundryLabs.Shared;

/// <summary>Simple local JSON persistence for learning. Not a production multi-node database.</summary>
public sealed class JsonConversationStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string _directory;

    public JsonConversationStore(string? directory = null)
    {
        _directory = directory
            ?? Environment.GetEnvironmentVariable("CHAT_HISTORY_DIR")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FoundryCSharpLabs", "Conversations");
        Directory.CreateDirectory(_directory);
    }

    private string FilePath(Guid id) => Path.Combine(_directory, $"{id:D}.json");

    public async Task<ConversationRecord> CreateAsync(string? title = null, CancellationToken ct = default)
    {
        ConversationRecord conversation = new();
        if (!string.IsNullOrWhiteSpace(title))
            conversation.Title = title.Trim()[..Math.Min(80, title.Trim().Length)];
        await SaveAsync(conversation, ct);
        return conversation;
    }

    public async Task<ConversationRecord?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var file = FilePath(id);
        if (!File.Exists(file)) return null;
        await using var input = File.OpenRead(file);
        return await JsonSerializer.DeserializeAsync<ConversationRecord>(input, Json, ct)
            ?? throw new InvalidDataException($"Empty conversation file: {file}");
    }

    public async Task<IReadOnlyList<ConversationSummary>> ListAsync(CancellationToken ct = default)
    {
        var results = new List<ConversationSummary>();
        foreach (string file in Directory.EnumerateFiles(_directory, "*.json"))
        {
            ct.ThrowIfCancellationRequested();
            await using var input = File.OpenRead(file);
            var conversation = await JsonSerializer.DeserializeAsync<ConversationRecord>(input, Json, ct)
                ?? throw new InvalidDataException($"Empty conversation file: {file}");
            results.Add(new ConversationSummary(conversation.Id, conversation.Title,
                conversation.UpdatedAtUtc, conversation.Messages.Count));
        }
        return results.OrderByDescending(x => x.UpdatedAtUtc).ToArray();
    }

    public async Task SaveAsync(ConversationRecord conversation, CancellationToken ct = default)
    {
        conversation.UpdatedAtUtc = DateTimeOffset.UtcNow;
        var file = FilePath(conversation.Id);
        var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var output = new FileStream(temporary, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(output, conversation, Json, ct);
                await output.FlushAsync(ct);
            }
            File.Move(temporary, file, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public async Task AppendExchangeAsync(ConversationRecord conversation, string userText,
        ResponseAnswer answer, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        conversation.Messages.Add(new ChatTurn("user", userText, now));
        conversation.Messages.Add(new ChatTurn("assistant", answer.Text, DateTimeOffset.UtcNow));
        conversation.LastResponseId = answer.ResponseId;
        await SaveAsync(conversation, ct);
    }
}
