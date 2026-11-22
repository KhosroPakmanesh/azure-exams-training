namespace AzureExamsTraining.AI103.Shared;

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
        string? textString = Console.ReadLine()?.Trim();
        return textString is null || textString.Equals(
            "quit",
            StringComparison.OrdinalIgnoreCase) ? null : textString;
    }

    public static void ShowError(Exception e)
    {
        Console.Error.WriteLine($"Error: {e.Message}\nCheck the README for configuration and access requirements.\n");
    }
}
