internal sealed record Profile(
    string Name,
    int MaxResults,
    string Ranker,
    bool RewriteQuery,
    double Threshold);
