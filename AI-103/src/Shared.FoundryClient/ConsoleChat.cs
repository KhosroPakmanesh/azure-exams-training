namespace FoundryLabs.Shared;

public static class ConsoleChat
{
    public static void Banner(string label)
    {
        Console.WriteLine($"=== {label} ===");
        Console.WriteLine("Enter questions, or type 'quit' to exit.\n");
    }

    public static string? ReadPrompt()
    {
        Console.Write("You: ");
        var text = Console.ReadLine()?.Trim();
        return text is null || text.Equals("quit", StringComparison.OrdinalIgnoreCase)
            ? null : text;
    }

    public static void ShowError(Exception e) =>
        Console.Error.WriteLine($"Error: {e.Message}\nCheck the README for configuration and access requirements.\n");
}
