using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("12: Prompt optimization (zero-shot, few-shot, parameters)");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/05-optimize-generative-ai-model-performance/12-prompt-optimization -- [--preview] [--dataset path.json] [--output results.csv]\n");
    }

    internal static void PrintPreview(
        TestCase[] caseTestCases,
        Variant[] variants)
    {
        Console.WriteLine($"Loaded {caseTestCases.Length} held-out examples. No API requests were made.\n");
        foreach (Variant? variant in variants)
        {
            Console.WriteLine(
                $"[{variant.Name}] temperature={variant.Temperature?.ToString() ?? "model default"}\n{variant.Instructions}\n");
        }
    }

    internal static void PrintReport(
        List<Result> rowResults,
        string outputPathString)
    {
        Console.WriteLine("\nSummary (accuracy excludes failed API requests):");
        foreach (IGrouping<string, Result>? groupStringResultIGrouping in rowResults.GroupBy(result => result.Variant))
        {
            Result[]? validResults = groupStringResultIGrouping.Where(result => result.Error.Length == 0).ToArray();
            Console.WriteLine(
                $"  {groupStringResultIGrouping.Key,-20} {validResults.Count(result => result.Correct)}/{validResults.Length} correct, " + $"{groupStringResultIGrouping.Count(result => result.Error.Length > 0)} errors, " + $"average latency {(validResults.Length == 0 ? "n/a" : validResults.Average(result => result.LatencyMs).ToString("F0") + " ms")}");
        }

        Console.WriteLine($"Results saved: {Path.GetFullPath(outputPathString)}");
    }
}
