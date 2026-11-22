using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("11: Margie's Travel (web_search + file_search)");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/04-use-generative-ai-tools/11-tools-chat -- <brochures-directory | brochure1.pdf ... | vs_existing_id>");
        Console.WriteLine("Try: What's happening in San Francisco next month?");
        Console.WriteLine("Then: What hotels does Margie's Travel offer there?\n");
    }

    internal static void PrintAnswer(JsonDocument responseJsonDocument)
    {
        FoundryToolRestClient.PrintAnswer(responseJsonDocument);
    }
}
