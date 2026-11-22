using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("18: Foundry guardrails and layered mitigations");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/06-responsible-ai-studio/18-content-safety-guardrails -- " + "[--preview] [--mode baseline|app] [--tag LABEL] [--review] [--show-output] " + "[--dataset cases.json] [--output report.csv]\n");
    }

    internal static void PrintPreview(
        GuardrailCase[] caseGuardrailCases,
        string modeString,
        string labelString)
    {
        Console.WriteLine($"{caseGuardrailCases.Length} test scenarios, mode={modeString}, tag={labelString}.");
        foreach (GuardrailCase? guardrailCase in caseGuardrailCases)
        {
            Console.WriteLine(
                $"  {guardrailCase.Id,-8} {guardrailCase.Category,-21} {guardrailCase.ExpectedBehavior}");
        }

        Console.WriteLine(
            "No API requests or output files. Guardrail configuration is managed in Foundry, not by this app.");
    }

    internal static void PrintReport(
        List<Result> rowResults,
        string outputString)
    {
        Console.WriteLine("\nObserved outcomes:");
        foreach (IGrouping<string, Result>? grpStringResultIGrouping in rowResults.GroupBy(result => result.Outcome))
        {
            Console.WriteLine($"  {grpStringResultIGrouping.Key}: {grpStringResultIGrouping.Count()}");
        }

        Console.WriteLine($"CSV: {Path.GetFullPath(outputString)}");
        Console.WriteLine(
            "Compare the same case IDs across configurations and manually review the actual responses.");
    }
}
