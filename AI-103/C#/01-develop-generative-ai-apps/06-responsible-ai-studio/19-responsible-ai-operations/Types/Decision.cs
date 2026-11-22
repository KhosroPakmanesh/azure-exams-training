internal sealed record Decision(
    string EventId,
    string Timestamp,
    string Type,
    int Severity,
    string Action);
