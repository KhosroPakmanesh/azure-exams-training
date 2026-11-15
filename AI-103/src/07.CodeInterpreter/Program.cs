using FoundryLabs.Shared;

ConsoleChat.Banner("07: code_interpreter tool");
Console.WriteLine("Try: Calculate the square root of 16 using Python.");
try
{
    using var client = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    string? previousId = null;
    object[] tools = [new { type = "code_interpreter", container = new { type = "auto" } }];

    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            using var response = await client.CreateResponseAsync(prompt, tools, previousId,
                "Use the python tool for calculations or data analysis. " +
                "Explain the result clearly.");
            FoundryToolRestClient.PrintAnswer(response);
            previousId = FoundryToolRestClient.RequireId(response);
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
