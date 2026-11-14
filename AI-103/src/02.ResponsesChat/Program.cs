using FoundryLabs.Shared;

ConsoleChat.Banner("02: Responses API (stateless)");
try
{
    var service = new FoundryResponseService(FoundryOptions.FromEnvironment());
    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            var response = await service.SendAsync(prompt); // Does NOT chain context.
            Console.WriteLine("Assistant: " + response.Text + "\n");
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
