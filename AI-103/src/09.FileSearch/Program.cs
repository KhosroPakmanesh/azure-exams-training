using FoundryLabs.Shared;

ConsoleChat.Banner("09: file_search tool");
Console.WriteLine("Usage: dotnet run --project src/09.FileSearch -- <document-directory | file1.pdf ... | vs_existing_id>\n");
try
{
    using var client = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    var storeId = await FoundryVectorStoreSetup.ResolveAsync(client, args, "file-search-lab");
    object[] tools = [new { type = "file_search", vector_store_ids = new[] { storeId } }];
    string? previousId = null;

    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            using var response = await client.CreateResponseAsync(prompt, tools, previousId,
                "Answer questions using the supplied documents. " +
                "When the documents do not contain the answer, say so.",
                include: ["file_search_call.results"]);
            FoundryToolRestClient.PrintAnswer(response);
            previousId = FoundryToolRestClient.RequireId(response);
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
