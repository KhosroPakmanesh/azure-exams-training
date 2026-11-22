using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("03: Context-aware Responses API");
    }

    internal static void PrintAnswer(string answerString)
    {
        Console.WriteLine("Assistant: " + answerString + "\n");
    }
}
