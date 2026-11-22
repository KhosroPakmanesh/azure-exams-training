using AzureExamsTraining.AI103.Shared;
using OpenAI.Chat;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
    ChatClient chatClient = FoundryClientFactory.CreateChatClient(foundryOptions);

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
            // Deliberately independent requests: no accumulated history in project 01.
            ChatCompletion answerChatCompletion = await chatClient.CompleteChatAsync([
                new SystemChatMessage(foundryOptions.Instructions),
                new UserChatMessage(promptString)
            ]);
            OutputPrinter.PrintAnswer(
                string.Concat(
                    answerChatCompletion.Content.Select(partChatMessageContentPart => partChatMessageContentPart.Text)));
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
