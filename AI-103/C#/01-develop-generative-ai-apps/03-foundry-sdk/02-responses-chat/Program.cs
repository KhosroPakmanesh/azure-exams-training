using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    var foundryResponseService = new FoundryResponseService(FoundryOptions.FromEnvironment());

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
            ResponseAnswer? responseResponseAnswer = await foundryResponseService.SendAsync(promptString); // Does NOT chain context.
            OutputPrinter.PrintAnswer(responseResponseAnswer.Text);
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
