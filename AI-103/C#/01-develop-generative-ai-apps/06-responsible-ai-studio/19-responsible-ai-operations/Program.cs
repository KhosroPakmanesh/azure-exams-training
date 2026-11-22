using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Read and order the validated synthetic events.
    string inputString = Option("--events") ?? Path.Combine(
        AppContext.BaseDirectory,
        "samples",
        "events.json");
    string outputString = Option("--output") ?? "responsible-ai-operations-report.json";
    IncidentEvent[] orderedIncidentEvents = await IncidentSequence.ReadAsync(inputString);
    if (args.Contains(
        "--preview",
        StringComparer.OrdinalIgnoreCase))
    {
        OutputPrinter.PrintPreview(orderedIncidentEvents);
        return;
    }

    // ------------------------------------------------------------------
    // INCIDENT SIMULATION
    // ------------------------------------------------------------------
    // 1. Apply events to the in-memory release state in time order.
    ReleaseState releaseState = new()
    {
        ReleaseStage = "canary",
        ActiveConfiguration = "policy-v2-candidate",
        PreviousConfiguration = "policy-v1-approved"
    };
    var actionLogDecisions = new List<Decision>();
    var abuseCountsStringIntDictionary = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (IncidentEvent incidentEvent in orderedIncidentEvents)
    {
        string actionString = IncidentSimulator.ApplyEvent(
            incidentEvent,
            releaseState,
            abuseCountsStringIntDictionary);
        actionLogDecisions.Add(
            new Decision(
                incidentEvent.Id,
                incidentEvent.Timestamp,
                incidentEvent.Kind,
                incidentEvent.Severity,
                actionString));
        Console.WriteLine($"{incidentEvent.Id,-8} {incidentEvent.Kind,-18} {actionString}");
    }

    // ------------------------------------------------------------------
    // RESULTS
    // ------------------------------------------------------------------
    // 1. Write the report and explain the observed results.
    var reportOperationsReport = new OperationsReport(
        "SIMULATION ONLY: no Azure configuration, users, or deployments were changed.",
        DateTimeOffset.UtcNow.ToString("O"),
        releaseState.ReleaseStage,
        releaseState.ActiveConfiguration,
        releaseState.PreviousConfiguration,
        releaseState.ResponseBlockEnabled,
        releaseState.OpenIncidents,
        releaseState.SimulatedBlockedActors.OrderBy(sString => sString).ToArray(),
        actionLogDecisions.ToArray());
    await File.WriteAllTextAsync(
        outputString,
        JsonSerializer.Serialize(
            reportOperationsReport,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    OutputPrinter.PrintReport(
        releaseState,
        outputString);
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}

string? Option(string nameString)
{
    int iInt = Array.FindIndex(
        args,
        aString => aString.Equals(
            nameString,
            StringComparison.OrdinalIgnoreCase));
    if (iInt < 0)
    {
        return null;
    }

    return iInt + 1 < args.Length ? args[iInt + 1] : throw new ArgumentException($"{nameString} requires a value.");
}
