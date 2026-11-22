using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load and validate the fictional risk register.
    string inputString = Option("--input") ?? Path.Combine(
        AppContext.BaseDirectory,
        "samples",
        "risks.json");
    string outputString = Option("--output") ?? "responsible-ai-risk-register.csv";
    Risk[] risks = await RiskRegister.ReadAsync(inputString);

    // ------------------------------------------------------------------
    // RANKING
    // ------------------------------------------------------------------
    // 1. Multiply likelihood by impact and order the risks.
    RankedRisk[] rankedRisks = RiskRegister.Rank(risks);
    OutputPrinter.PrintRankedRisks(rankedRisks);
    if (args.Contains(
        "--preview",
        StringComparer.OrdinalIgnoreCase))
    {
        Console.WriteLine("Preview only. Nothing written; no Azure connection required.");
        return;
    }

    // ------------------------------------------------------------------
    // RESULTS
    // ------------------------------------------------------------------
    // 1. Write the report and explain the observed results.
    await RiskRegister.WriteCsvAsync(
        rankedRisks,
        outputString);
    OutputPrinter.PrintReport(outputString);
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}

string? Option(string nameString)
{
    int iInt = Array.FindIndex(
        args,
        aString => aString.Equals(
            nameString,
            StringComparison.OrdinalIgnoreCase));
    if (iInt < 0)
    {
        return null;
    }

    return iInt + 1 < args.Length ? args[iInt + 1] : throw new ArgumentException($"{nameString} requires a value.");
}
