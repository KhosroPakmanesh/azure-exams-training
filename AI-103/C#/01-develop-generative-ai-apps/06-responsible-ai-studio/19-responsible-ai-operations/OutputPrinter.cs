using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("19: Manage responsible AI releases (offline incident simulation)");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/06-responsible-ai-studio/19-responsible-ai-operations -- " +
            "[--preview] [--events events.json] [--output state.json]\n");
    }

    internal static void PrintPreview(IncidentEvent[] orderedIncidentEvents)
    {
        Console.WriteLine($"{orderedIncidentEvents.Length} synthetic events, in chronological order:");
        foreach (IncidentEvent? incidentEvent in orderedIncidentEvents)
        {
            Console.WriteLine(
                $"{incidentEvent.Id,-8} {incidentEvent.Kind,-18} severity={incidentEvent.Severity} verified={incidentEvent.Verified}");
        }

        Console.WriteLine("No Azure calls, policy updates, or output files.");
    }

    internal static void PrintReport(
        ReleaseState releaseState,
        string outputString)
    {
        Console.WriteLine($"\nFinal simulated release stage: {releaseState.ReleaseStage}");
        Console.WriteLine($"Final simulated configuration: {releaseState.ActiveConfiguration}");
        Console.WriteLine($"Report: {Path.GetFullPath(outputString)}");
        Console.WriteLine(
            "No real rollback or user-block action happened. Implement operational controls separately.");
    }
}
