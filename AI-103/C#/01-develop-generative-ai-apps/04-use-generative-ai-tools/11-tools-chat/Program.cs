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
        "travel-brochures",
        pdfOnly: true);
    object[] toolObjects = [
        new
    {
        type = "file_search",
        vector_store_ids = new[]
        {
            storeIdString
        }
    },
        new
    {
        type = "web_search"
    }

    ];
    string? previousIdString = null;
    const string instructionsString =
        "You are a travel assistant that provides information on travel services available from Margie's Travel. "
        + "Answer questions about services offered by Margie's Travel using the provided travel brochures. "
        + "Search the web for general information about destinations or current travel advice. "
        + "Do not invent travel services that are not documented in the brochures.";

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
                instructionsString);
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
