internal sealed record Result(
    string Id,
    string Category,
    string Tag,
    string Mode,
    string Outcome,
    string ReviewerLabel,
    long ElapsedMs,
    string ResponseId,
    string Error);
