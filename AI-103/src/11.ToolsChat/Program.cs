using FoundryLabs.Shared;

ConsoleChat.Banner("11: Margie's Travel (web_search + file_search)");
Console.WriteLine("Usage: dotnet run --project src/11.ToolsChat -- <brochures-directory | brochure1.pdf ... | vs_existing_id>");
Console.WriteLine("Try: What's happening in San Francisco next month?");
Console.WriteLine("Then: What hotels does Margie's Travel offer there?\n");
try
{
    using var client = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    var storeId = await FoundryVectorStoreSetup.ResolveAsync(client, args, "travel-brochures", pdfOnly: true);
    object[] tools =
    [
        new { type = "file_search", vector_store_ids = new[] { storeId } },
        new { type = "web_search" }
    ];
    string? previousId = null;
    const string instructions =
        "You are a travel assistant that provides information on travel services available from Margie's Travel. " +
        "Answer questions about services offered by Margie's Travel using the provided travel brochures. " +
        "Search the web for general information about destinations or current travel advice. " +
        "Do not invent travel services that are not documented in the brochures.";

    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            using var response = await client.CreateResponseAsync(prompt, tools, previousId,
                instructions);
            FoundryToolRestClient.PrintAnswer(response);
            previousId = FoundryToolRestClient.RequireId(response);
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
