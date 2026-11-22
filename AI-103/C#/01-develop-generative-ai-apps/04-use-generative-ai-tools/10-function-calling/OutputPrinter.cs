using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("10: function tool (get_time)");
        Console.WriteLine("Try: What is the current local time?\n");
    }

    internal static void PrintAnswer(JsonDocument responseJsonDocument)
    {
        FoundryToolRestClient.PrintAnswer(responseJsonDocument);
    }
}
