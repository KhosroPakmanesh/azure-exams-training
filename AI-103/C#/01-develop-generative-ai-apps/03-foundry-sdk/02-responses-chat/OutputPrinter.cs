using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("02: Responses API (stateless)");
    }

    internal static void PrintAnswer(string answerString)
    {
        Console.WriteLine("Assistant: " + answerString + "\n");
    }
}
