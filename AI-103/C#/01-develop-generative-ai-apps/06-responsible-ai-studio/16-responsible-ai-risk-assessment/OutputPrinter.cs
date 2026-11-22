using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("16: Map potential harms / risk assessment");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/06-responsible-ai-studio/16-responsible-ai-risk-assessment -- [--preview] " + "[--input risk-register.json] [--output risk-register.csv]\n");
    }

    internal static void PrintRankedRisks(RankedRisk[] rankedRankedRisks)
    {
        Console.WriteLine("TRAVEL ASSISTANT RISK REGISTER (illustrative scoring, not an audit)\n");
        Console.WriteLine(
            "{0,-8} {1,-22} {2,2} {3,2} {4,5}  Priority  Harm",
            "ID",
            "Category",
            "L",
            "I",
            "Score");
        foreach (RankedRisk? itemRankedRisk in rankedRankedRisks)
        {
            Console.WriteLine(
                $"{itemRankedRisk.Risk.Id,-8} {Short(
                    itemRankedRisk.Risk.Category,
                    22),-22} " + $"{itemRankedRisk.Risk.Likelihood,2} {itemRankedRisk.Risk.Impact,2} {itemRankedRisk.Score,5}  {itemRankedRisk.Priority,-8}  " + Short(
                        itemRankedRisk.Risk.Harm,
                        65));
        }

        Console.WriteLine(
            $"\nHigh: {rankedRankedRisks.Count(rankedRisk => rankedRisk.Priority == "HIGH")}, " + $"medium: {rankedRankedRisks.Count(rankedRisk => rankedRisk.Priority == "MEDIUM")}, " + $"low: {rankedRankedRisks.Count(rankedRisk => rankedRisk.Priority == "LOW")}");
    }

    private static string Short(
        string? sString,
        int limitInt)
    {
        return string.IsNullOrEmpty(sString) ? "" : sString.Length <= limitInt ? sString : sString[..(limitInt - 3)] + "...";
    }

    internal static void PrintReport(string outputString)
    {
        Console.WriteLine($"CSV exported to {Path.GetFullPath(outputString)}");
        Console.WriteLine(
            "Discuss scores with stakeholders; likelihood x impact is a teaching aid, not a risk certification.");
    }
}
