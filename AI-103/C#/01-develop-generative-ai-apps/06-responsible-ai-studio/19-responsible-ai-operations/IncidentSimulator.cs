internal static class IncidentSimulator
{
    internal static string ApplyEvent(
        IncidentEvent incidentEvent,
        ReleaseState releaseState,
        Dictionary<string, int> abuseCountsStringIntDictionary)
    {
        // This program only simulates platform changes. No endpoint or credential is used.
        string actionString;
        switch (incidentEvent.Kind)
        {
            case "feedback":
                actionString = incidentEvent.Severity >= 3 ? "Open review ticket; human triage required" : "Aggregate satisfaction signal; no automated intervention";
                if (incidentEvent.Severity >= 3)
                {
                    releaseState.OpenIncidents++;
                }

                break;
            case "abuse":
                if (string.IsNullOrWhiteSpace(incidentEvent.ActorToken))
                {
                    throw new InvalidDataException($"{incidentEvent.Id} abuse event must have a pseudonymous actor token.");
                }

                int countInt = abuseCountsStringIntDictionary.TryGetValue(
                    incidentEvent.ActorToken,
                    out int priorInt) ? priorInt + 1 : 1;
                abuseCountsStringIntDictionary[incidentEvent.ActorToken] = countInt;
                if (countInt >= 2 && incidentEvent.Verified)
                {
                    releaseState.SimulatedBlockedActors.Add(incidentEvent.ActorToken);
                    actionString = "Simulate blocking verified repeat-abuse actor; human approval required in production";
                }
                else
                {
                    actionString = "Watch for repeat events; no block yet";
                }

                break;
            case "verified_harm":
                if (incidentEvent.Verified && incidentEvent.Severity >= 4)
                {
                    releaseState.ReleaseStage = "paused";
                    releaseState.ResponseBlockEnabled = true;
                    releaseState.ActiveConfiguration = releaseState.PreviousConfiguration;
                    releaseState.OpenIncidents++;
                    actionString = "SIMULATED pause + response block + rollback to previous approved configuration";
                }
                else
                {
                    releaseState.OpenIncidents++;
                    actionString = "Escalate for human verification; no automatic rollback";
                }

                break;
            case "release_approval":
                if (incidentEvent.Verified && releaseState.ReleaseStage != "paused")
                {
                    releaseState.ReleaseStage = "limited-release";
                    actionString = "Simulate approved limited rollout; monitor before general availability";
                }
                else
                {
                    actionString = "Hold release; approvals cannot override a paused incident state";
                }

                break;
            default:
                throw new InvalidDataException($"Unexpected event kind: {incidentEvent.Kind}");
        }

        return actionString;
    }
}
