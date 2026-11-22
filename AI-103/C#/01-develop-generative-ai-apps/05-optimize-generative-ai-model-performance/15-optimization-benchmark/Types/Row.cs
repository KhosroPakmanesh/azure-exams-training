internal sealed record Row(
    string CaseId,
    string Task,
    string Strategy,
    string Expected,
    string Output,
    bool Correct,
    long LatencyMs,
    int? InputTokens,
    int? OutputTokens,
    string Error);
