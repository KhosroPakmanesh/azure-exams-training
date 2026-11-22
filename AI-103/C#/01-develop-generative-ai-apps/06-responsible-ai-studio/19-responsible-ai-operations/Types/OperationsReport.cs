internal sealed record OperationsReport(
    string Disclaimer,
    string CreatedUtc,
    string ReleaseStage,
    string ActiveConfiguration,
    string PreviousConfiguration,
    bool ResponseBlockEnabled,
    int OpenIncidents,
    string[] SimulatedBlockedActors,
    Decision[] Decisions);
