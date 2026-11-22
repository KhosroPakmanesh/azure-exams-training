using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("09: file_search tool");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/04-use-generative-ai-tools/09-file-search -- <document-directory | file1.pdf ... | vs_existing_id>\n");
    }

    internal static void PrintAnswer(JsonDocument responseJsonDocument)
    {
        FoundryToolRestClient.PrintAnswer(responseJsonDocument);
    }
}
