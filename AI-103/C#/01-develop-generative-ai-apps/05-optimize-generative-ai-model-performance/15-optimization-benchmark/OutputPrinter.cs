using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("15: Optimization benchmark (two comparable task tracks)");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/05-optimize-generative-ai-model-performance/15-optimization-benchmark -- [--preview] [--store vs_...] " + "[--fine-tuned-deployment NAME] [--limit N] [--output report.csv]\n");
    }

    internal static void PrintPreview(
        IntentCase[] intentCases,
        FactCase[] factCases)
    {
        Console.WriteLine(
            $"Offline preview: {intentCases.Length} intent tests and {factCases.Length} policy-fact tests.");
        Console.WriteLine("Intent: baseline vs few-shot; plus fine-tuned if --fine-tuned-deployment is provided.");
        Console.WriteLine(
            "Policy facts: baseline vs improved prompt; plus grounded file_search if --store is provided.");
        Console.WriteLine(
            "No requests made. Comparisons are grouped by task, NOT pooled into one misleading score.");
    }

    internal static void PrintReport(
        List<Row> resultRows,
        string outputString)
    {
        Console.WriteLine("\nSummary (same-task comparisons only):");
        foreach (IGrouping<(string Task, string Strategy), Row>? groupTupleRowIGrouping in resultRows.GroupBy(row => (row.Task, row.Strategy)))
        {
            Row[]? successfulRows = groupTupleRowIGrouping.Where(row => row.Error.Length == 0).ToArray();
            Console.WriteLine(
                $"  {groupTupleRowIGrouping.Key.Task,-12} {groupTupleRowIGrouping.Key.Strategy,-28} " + $"{successfulRows.Count(row => row.Correct)}/{successfulRows.Length} correct, " + $"{groupTupleRowIGrouping.Count(row => row.Error.Length != 0)} errors, " + $"mean latency {(successfulRows.Length == 0 ? "n/a" : successfulRows.Average(row => row.LatencyMs).ToString("F0") + " ms")}");
        }

        Console.WriteLine($"CSV: {Path.GetFullPath(outputString)}");
        Console.WriteLine(
            "Note: exact labels and phrase checks are simple checks, not a comprehensive quality evaluation.");
    }
}
