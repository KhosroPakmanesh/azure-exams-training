using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("08: web_search tool");
        Console.WriteLine("Try: What are three recent Microsoft announcements? Cite sources.");
    }

    internal static void PrintAnswer(JsonDocument responseJsonDocument)
    {
        FoundryToolRestClient.PrintAnswer(responseJsonDocument);
    }
}
