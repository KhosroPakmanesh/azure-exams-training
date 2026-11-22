using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("17: Measure potential harms (manual evaluation)");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/06-responsible-ai-studio/17-harm-evaluation -- [--preview | --live] " + "[--dataset tests.json] [--limit N] [--output evaluation.csv]\n");
    }

    internal static void PrintPreview(HarmTest[] testHarmTests)
    {
        Console.WriteLine(
            $"Loaded {testHarmTests.Length} scenarios. The program uses human review rather than keyword safety guesses.");
        foreach (HarmTest? harmTest in testHarmTests)
        {
            Console.WriteLine($"{harmTest.Id,-8} {harmTest.Category,-22} {harmTest.ExpectedBehavior}");
        }

        Console.WriteLine("No API calls and no files written.");
    }

    internal static void PrintReport(
        List<Evaluation> resultEvaluations,
        string outputString)
    {
        Console.WriteLine("\nSummary by category (pass/fail only; uncertain and errors excluded from rate):");
        foreach (IGrouping<string, Evaluation>? groupStringEvaluationIGrouping in resultEvaluations.GroupBy(evaluation => evaluation.Category))
        {
            Evaluation[]? reviewedEvaluations = groupStringEvaluationIGrouping.Where(evaluation => evaluation.Label is "pass" or "fail").ToArray();
            string rateString = reviewedEvaluations.Length == 0 ? "n/a" : $"{(100.0 * reviewedEvaluations.Count(evaluation => evaluation.Label == "fail") / reviewedEvaluations.Length):F1}%";
            Console.WriteLine(
                $"  {groupStringEvaluationIGrouping.Key,-22} {reviewedEvaluations.Length,2} reviewed, " + $"failure rate={rateString}, uncertain={groupStringEvaluationIGrouping.Count(evaluation => evaluation.Label == "uncertain")}, " + $"errors={groupStringEvaluationIGrouping.Count(evaluation => evaluation.Label == "error")}");
        }

        Console.WriteLine($"CSV: {Path.GetFullPath(outputString)}");
        Console.WriteLine("Manual review is subjective. Calibrate a rubric and repeat on a larger, diverse set.");
    }
}
