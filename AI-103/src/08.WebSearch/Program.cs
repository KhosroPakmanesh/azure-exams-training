using FoundryLabs.Shared;

ConsoleChat.Banner("08: web_search tool");
Console.WriteLine("Try: What are three recent Microsoft announcements? Cite sources.");
try
{
    using var client = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    string? previousId = null;
    object[] tools = [new { type = "web_search" }];

    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            using var response = await client.CreateResponseAsync(prompt, tools, previousId,
                "Use web search for current information. Prefer authoritative sources and cite them.");
            FoundryToolRestClient.PrintAnswer(response);
            previousId = FoundryToolRestClient.RequireId(response);
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
