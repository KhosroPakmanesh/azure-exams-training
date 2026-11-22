internal sealed record Result(
    string Variant,
    string Id,
    string Expected,
    string Predicted,
    string Raw,
    bool Correct,
    long LatencyMs,
    int? InputTokens,
    int? OutputTokens,
    string Error);
