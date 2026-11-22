using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("05: Async chatbot with cancellation");
        Console.WriteLine("This version awaits all model calls and lets you cancel a request with Ctrl+C.\n");
    }

    internal static void PrintAnswer(string answerString)
    {
        Console.WriteLine("Assistant: " + answerString + "\n");
    }

    internal static void PrintSending()
    {
        Console.WriteLine("Sending asynchronous request ...");
    }

    internal static void PrintCancellation()
    {
        Console.WriteLine("Request canceled.\n");
    }
}
