internal sealed record HarmTest(
    string Id,
    string Category,
    string Prompt,
    string ExpectedBehavior,
    string SampleReviewerLabel);
