internal sealed record Evaluation(
    string Id,
    string Category,
    string ExpectedBehavior,
    string Source,
    string Label,
    long LatencyMs,
    string ResponseId,
    string Error);
