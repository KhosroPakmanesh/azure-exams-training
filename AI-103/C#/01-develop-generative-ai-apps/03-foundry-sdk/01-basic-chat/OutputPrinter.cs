using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("01: Chat Completions API");
    }

    internal static void PrintAnswer(string answerString)
    {
        Console.WriteLine("Assistant: " + answerString + "\n");
    }
}
