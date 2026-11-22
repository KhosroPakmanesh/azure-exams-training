using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("04: Streaming Responses API");
    }

    internal static void BeginAnswer()
    {
        Console.Write("Assistant: ");
    }

    internal static void EndAnswer()
    {
        Console.WriteLine("\n");
    }

    internal static void WriteDelta(string deltaString)
    {
        Console.Write(deltaString);
    }
}
