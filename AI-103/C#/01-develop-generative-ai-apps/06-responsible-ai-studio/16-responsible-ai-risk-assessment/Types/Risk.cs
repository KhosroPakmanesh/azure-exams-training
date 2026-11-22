internal sealed record Risk(
    string Id,
    string Category,
    string Scenario,
    string Harm,
    int Likelihood,
    int Impact,
    string Owner,
    string Mitigation,
    string Evidence,
    string Status);
