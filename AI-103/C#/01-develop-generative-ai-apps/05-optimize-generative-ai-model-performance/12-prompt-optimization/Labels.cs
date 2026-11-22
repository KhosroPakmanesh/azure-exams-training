internal static class Labels
{
    private static readonly HashSet<string> ValueStrings = [
        "BOOKING",
        "CANCELLATION",
        "BAGGAGE",
        "OTHER"
    ];
    public static bool Contains(string labelString)
    {
        return ValueStrings.Contains(labelString);
    }
}
