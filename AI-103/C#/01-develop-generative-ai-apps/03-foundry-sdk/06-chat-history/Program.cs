using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
    var foundryResponseService = new FoundryResponseService(foundryOptions);
    var storeJsonConversationStore = new JsonConversationStore();
    ConversationRecord? activeConversationRecord = null;

    // ------------------------------------------------------------------
    // CHAT TURNS
    // ------------------------------------------------------------------
    // 1. Read a prompt, call the model, and show the answer before reading the next prompt.
    while (true)
    {
        Console.Write(activeConversationRecord is null ? "history> " : $"{activeConversationRecord.Title}> ");
        string? inputString = Console.ReadLine()?.Trim();
        if (inputString is null || inputString.Equals(
            "quit",
            StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (inputString.Length == 0)
        {
            continue;
        }

        try
        {
            if (inputString.Equals(
                "list",
                StringComparison.OrdinalIgnoreCase))
            {
                foreach (ConversationSummary? conversationSummary in await storeJsonConversationStore.ListAsync())
                {
                    Console.WriteLine(
                        $"{conversationSummary.Id}  {conversationSummary.Title}  ({conversationSummary.MessageCount} messages)");
                }

                continue;
            }

            if (inputString.Equals(
                "new",
                StringComparison.OrdinalIgnoreCase) || inputString.StartsWith(
                    "new ",
                    StringComparison.OrdinalIgnoreCase))
            {
                string? titleString = inputString.Length > 4 ? inputString[4..] : null;
                activeConversationRecord = await storeJsonConversationStore.CreateAsync(titleString);
                Console.WriteLine($"Created {activeConversationRecord.Id}\n");
                continue;
            }

            if (inputString.StartsWith(
                "use ",
                StringComparison.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(
                    inputString[4..],
                    out Guid idGuid))
                {
                    Console.WriteLine("Enter a valid conversation GUID.\n");
                    continue;
                }

                activeConversationRecord = await storeJsonConversationStore.GetAsync(idGuid);
                Console.WriteLine(
                    activeConversationRecord is null ? "Conversation not found.\n" : $"Resumed: {activeConversationRecord.Title}\n");
                continue;
            }

            if (inputString.Equals(
                "show",
                StringComparison.OrdinalIgnoreCase))
            {
                if (activeConversationRecord is null)
                {
                    Console.WriteLine("No active conversation.\n");
                    continue;
                }

                foreach (ChatTurn? turnChatTurn in activeConversationRecord.Messages)
                {
                    Console.WriteLine($"{turnChatTurn.Role}: {turnChatTurn.Content}");
                }

                Console.WriteLine();
                continue;
            }

            activeConversationRecord ??= await storeJsonConversationStore.CreateAsync("Chat " + DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"));
            ResponseAnswer? answerResponseAnswer = await foundryResponseService.SendAsync(
                inputString,
                activeConversationRecord.LastResponseId);
            await storeJsonConversationStore.AppendExchangeAsync(
                activeConversationRecord,
                inputString,
                answerResponseAnswer);
            OutputPrinter.PrintAnswer(answerResponseAnswer.Text);
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
