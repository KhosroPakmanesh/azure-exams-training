internal sealed record Question(
    string Id,
    string Text,
    string? ExpectedPhrase,
    string? ExpectedFile);
