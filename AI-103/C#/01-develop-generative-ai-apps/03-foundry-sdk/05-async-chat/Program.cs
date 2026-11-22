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

        using var requestCancellationCancellationTokenSource = new CancellationTokenSource();
        ConsoleCancelEventHandler consoleCancelEventHandler = (
            _,
            eventArgConsoleCancelEventArgs) =>
        {
            eventArgConsoleCancelEventArgs.Cancel = true;
            requestCancellationCancellationTokenSource.Cancel();
        };
        Console.CancelKeyPress += consoleCancelEventHandler;
        try
        {
            OutputPrinter.PrintSending();
            ResponseAnswer? answerResponseAnswer = await foundryResponseService.SendAsync(
                promptString,
                previousResponseIdString,
                requestCancellationCancellationTokenSource.Token);
            OutputPrinter.PrintAnswer(answerResponseAnswer.Text);
            previousResponseIdString = answerResponseAnswer.ResponseId;
        }
        catch (OperationCanceledException)
        {
            OutputPrinter.PrintCancellation();
        }
        catch (Exception exception)
        {
            ConsoleChat.ShowError(exception);
        }
        finally
        {
            Console.CancelKeyPress -= consoleCancelEventHandler;
        }
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}
