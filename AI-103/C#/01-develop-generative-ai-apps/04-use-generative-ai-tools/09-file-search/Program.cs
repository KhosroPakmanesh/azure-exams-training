using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    using var foundryToolRestClient = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    string? storeIdString = await FoundryVectorStoreSetup.ResolveAsync(
        foundryToolRestClient,
        args,
        "file-search-lab");
    object[] toolObjects = [
        new
    {
        type = "file_search",
        vector_store_ids = new[]
        {
            storeIdString
        }
    }

    ];
    string? previousIdString = null;

    // ------------------------------------------------------------------
    // CHAT TURNS
    // ------------------------------------------------------------------
    // 1. Read a prompt, call the model, and show the answer before reading the next prompt.
    while (true)
    {
        string? promptString = ConsoleChat.ReadPrompt();
        if (promptString is null)
        {
            break;
        }

        if (promptString.Length == 0)
        {
            continue;
        }

        try
        {
            using System.Text.Json.JsonDocument? responseJsonDocument = await foundryToolRestClient.CreateResponseAsync(
                promptString,
                toolObjects,
                previousIdString,
                "Answer questions using the supplied documents. " + "When the documents do not contain the answer, say so.",
                include: ["file_search_call.results"]);
            OutputPrinter.PrintAnswer(responseJsonDocument);
            previousIdString = FoundryToolRestClient.RequireId(responseJsonDocument);
        }
        catch (Exception exception)
        {
            ConsoleChat.ShowError(exception);
        }
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}
