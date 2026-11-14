using FoundryLabs.Shared;
using OpenAI.Chat;

ConsoleChat.Banner("01: Chat Completions API");
try
{
    var settings = FoundryOptions.FromEnvironment();
    ChatClient client = FoundryClientFactory.CreateChatClient(settings);
    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        try
        {
            // Deliberately independent requests: no accumulated history in project 01.
            ChatCompletion answer = await client.CompleteChatAsync(
            [
                new SystemChatMessage(settings.Instructions),
                new UserChatMessage(prompt)
            ]);
            Console.WriteLine("Assistant: " + string.Concat(answer.Content.Select(part => part.Text)) + "\n");
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
