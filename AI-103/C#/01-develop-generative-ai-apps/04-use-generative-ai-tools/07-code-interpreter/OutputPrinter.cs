using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("07: code_interpreter tool");
        Console.WriteLine("Try: Calculate the square root of 16 using Python.");
    }

    internal static void PrintAnswer(JsonDocument responseJsonDocument)
    {
        FoundryToolRestClient.PrintAnswer(responseJsonDocument);
    }
}
