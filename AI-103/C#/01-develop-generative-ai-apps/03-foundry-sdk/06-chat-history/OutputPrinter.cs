using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        Console.WriteLine("=== 06: Persistent chat history ===");
        Console.WriteLine("Commands: new [title], list, use <conversation-id>, show, quit");
        Console.WriteLine("Otherwise type a message to chat.\n");
    }

    internal static void PrintAnswer(string answerString)
    {
        Console.WriteLine("Assistant: " + answerString + "\n");
    }
}
