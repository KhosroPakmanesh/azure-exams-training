using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    using var foundryToolRestClient = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    string? previousIdString = null;
    object[] toolObjects = [
        new
    {
        type = "code_interpreter",
        container = new
        {
            type = "auto"
        }
    }

    ];

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
            using JsonDocument? responseJsonDocument =
                await foundryToolRestClient.CreateResponseAsync(
                promptString,
                toolObjects,
                previousIdString,
                "Use the python tool for calculations or data analysis. " + "Explain the result clearly.");
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
