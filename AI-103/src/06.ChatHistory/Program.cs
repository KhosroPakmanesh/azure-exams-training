using FoundryLabs.Shared;

Console.WriteLine("=== 06: Persistent chat history ===");
Console.WriteLine("Commands: new [title], list, use <conversation-id>, show, quit");
Console.WriteLine("Otherwise type a message to chat.\n");
try
{
    var settings = FoundryOptions.FromEnvironment();
    var service = new FoundryResponseService(settings);
    var store = new JsonConversationStore();
    ConversationRecord? active = null;
    while (true)
    {
        Console.Write(active is null ? "history> " : $"{active.Title}> ");
        string? input = Console.ReadLine()?.Trim();
        if (input is null || input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;
        if (input.Length == 0) continue;
        try
        {
            if (input.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var c in await store.ListAsync())
                    Console.WriteLine($"{c.Id}  {c.Title}  ({c.MessageCount} messages)");
                continue;
            }
            if (input.Equals("new", StringComparison.OrdinalIgnoreCase) || input.StartsWith("new ", StringComparison.OrdinalIgnoreCase))
            {
                var title = input.Length > 4 ? input[4..] : null;
                active = await store.CreateAsync(title);
                Console.WriteLine($"Created {active.Id}\n");
                continue;
            }
            if (input.StartsWith("use ", StringComparison.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(input[4..], out var id))
                {
                    Console.WriteLine("Enter a valid conversation GUID.\n");
                    continue;
                }
                active = await store.GetAsync(id);
                Console.WriteLine(active is null ? "Conversation not found.\n" : $"Resumed: {active.Title}\n");
                continue;
            }
            if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
            {
                if (active is null) { Console.WriteLine("No active conversation.\n"); continue; }
                foreach (var turn in active.Messages)
                    Console.WriteLine($"{turn.Role}: {turn.Content}");
                Console.WriteLine();
                continue;
            }
            active ??= await store.CreateAsync("Chat " + DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"));
            var answer = await service.SendAsync(input, active.LastResponseId);
            await store.AppendExchangeAsync(active, input, answer);
            Console.WriteLine("Assistant: " + answer.Text + "\n");
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
