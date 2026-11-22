internal sealed record Result(
    string QuestionId,
    string Profile,
    string? Expected,
    string Answer,
    bool? RetrievalHit,
    bool? AnswerContainsFact,
    int Chunks,
    long LatencyMs,
    string Error);
