internal sealed class ReleaseState
{
    public string ReleaseStage { get; set; } = "canary";
    public string ActiveConfiguration { get; set; } = "policy-v2-candidate";
    public string PreviousConfiguration { get; set; } = "policy-v1-approved";
    public bool ResponseBlockEnabled
    {
        get; set;
    }
    public int OpenIncidents
    {
        get; set;
    }
    public HashSet<string> SimulatedBlockedActors { get; } = new(StringComparer.Ordinal);
}
