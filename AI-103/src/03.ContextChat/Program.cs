using FoundryLabs.Shared;

ConsoleChat.Banner("03: Context-aware Responses API");
try
{
    var service = new FoundryResponseService(FoundryOptions.FromEnvironment());
    string? previousResponseId = null;
    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            var response = await service.SendAsync(prompt, previousResponseId);
            Console.WriteLine("Assistant: " + response.Text + "\n");
            previousResponseId = response.ResponseId;
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
