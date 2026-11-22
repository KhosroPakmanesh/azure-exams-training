using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("13: RAG retrieval optimization");
        Console.WriteLine(
            "Usage: dotnet run --project 01-develop-generative-ai-apps/05-optimize-generative-ai-model-performance/13-rag-optimization -- --sample");
        Console.WriteLine(
            "   or: dotnet run --project 01-develop-generative-ai-apps/05-optimize-generative-ai-model-performance/13-rag-optimization -- <vs_existing_id | documents-directory | file1.md ...>\n");
    }

    internal static void PrintStore(string storeIdString)
    {
        Console.WriteLine($"Vector store: {storeIdString}");
        Console.WriteLine(
            "This project uses explicit vector-store search, not the file_search tool from Project 09.");
    }
}
