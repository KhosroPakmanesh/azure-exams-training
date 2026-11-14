namespace FoundryLabs.Shared;

public sealed record ChatTurn(string Role, string Content, DateTimeOffset CreatedAtUtc);

public sealed class ConversationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "New conversation";
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? LastResponseId { get; set; }
    public List<ChatTurn> Messages { get; set; } = [];
}

public sealed record ConversationSummary(Guid Id, string Title, DateTimeOffset UpdatedAtUtc, int MessageCount);
public sealed record StartConversationRequest(string? Title);
public sealed record SendMessageRequest(string Message);
public sealed record SendMessageResult(Guid ConversationId, string Text, string ResponseId);
