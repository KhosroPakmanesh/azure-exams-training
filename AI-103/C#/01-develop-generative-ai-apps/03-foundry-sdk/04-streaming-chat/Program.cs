using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    var foundryResponseService = new FoundryResponseService(FoundryOptions.FromEnvironment());
    string? previousResponseIdString = null;

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

        OutputPrinter.BeginAnswer();
        try
        {
            await foreach (StreamChunk? chunkStreamChunk in foundryResponseService.StreamAsync(
                promptString,
                previousResponseIdString))
            {
                if (chunkStreamChunk.Completed)
                {
                    previousResponseIdString = chunkStreamChunk.ResponseId;
                }
                else
                {
                    OutputPrinter.WriteDelta(chunkStreamChunk.Text);
                }
            }

            OutputPrinter.EndAnswer();
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            ConsoleChat.ShowError(exception);
        }
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}
