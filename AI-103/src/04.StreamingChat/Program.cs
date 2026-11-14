using FoundryLabs.Shared;

ConsoleChat.Banner("04: Streaming Responses API");
try
{
    var service = new FoundryResponseService(FoundryOptions.FromEnvironment());
    string? previousResponseId = null;
    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        Console.Write("Assistant: ");
        try
        {
            await foreach (var chunk in service.StreamAsync(prompt, previousResponseId))
            {
                if (chunk.Completed)
                {
                    previousResponseId = chunk.ResponseId;
                }
                else
                {
                    Console.Write(chunk.Text);
                }
            }
            Console.WriteLine("\n");
        }
        catch (Exception ex) { Console.WriteLine(); ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
