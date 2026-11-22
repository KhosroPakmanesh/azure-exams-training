internal sealed record IncidentEvent(
    string Id,
    string Timestamp,
    string Kind,
    int Severity,
    string ActorToken,
    string Summary,
    bool Verified);
