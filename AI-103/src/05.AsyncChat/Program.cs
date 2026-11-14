using FoundryLabs.Shared;

ConsoleChat.Banner("05: Async chatbot with cancellation");
Console.WriteLine("This version awaits all model calls and lets you cancel a request with Ctrl+C.\n");
try
{
    var service = new FoundryResponseService(FoundryOptions.FromEnvironment());
    string? previousResponseId = null;
    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;

        using var requestCancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            requestCancellation.Cancel();
        };
        Console.CancelKeyPress += handler;
        try
        {
            Console.WriteLine("Sending asynchronous request ...");
            var answer = await service.SendAsync(prompt, previousResponseId, requestCancellation.Token);
            Console.WriteLine("Assistant: " + answer.Text + "\n");
            previousResponseId = answer.ResponseId;
        }
        catch (OperationCanceledException) { Console.WriteLine("Request canceled.\n"); }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
        finally { Console.CancelKeyPress -= handler; }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
