using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    string? samplePathString = Path.Combine(
        AppContext.BaseDirectory,
        "samples",
        "benchmark.json");
    BenchDataset? testsBenchDataset = JsonSerializer.Deserialize<BenchDataset>(
        await File.ReadAllTextAsync(samplePathString),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("Benchmark dataset is empty.");
    if (testsBenchDataset.Intents is null || testsBenchDataset.Facts is null || testsBenchDataset.Intents.Length == 0 || testsBenchDataset.Facts.Length == 0)
    {
        throw new InvalidDataException("Both task tracks need evaluation cases.");
    }

    string? storeIdString = Option("--store");
    string? fineTunedString = Option("--fine-tuned-deployment");
    string outputString = Option("--output") ?? "optimization-benchmark.csv";
    int limitInt = int.TryParse(
        Option("--limit"),
        out int nInt) && nInt > 0 ? nInt : int.MaxValue;
    IntentCase[]? intentCases = testsBenchDataset.Intents.Take(limitInt).ToArray();
    FactCase[]? factCases = testsBenchDataset.Facts.Take(limitInt).ToArray();
    if (args.Contains(
        "--preview",
        StringComparer.OrdinalIgnoreCase))
    {
        OutputPrinter.PrintPreview(
            intentCases,
            factCases);
        return;
    }

    // ------------------------------------------------------------------
    // BENCHMARK
    // ------------------------------------------------------------------
    // 1. Measure each strategy on its own task.
    FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
    using var foundryToolRestClient = new FoundryToolRestClient(foundryOptions);
    var resultRows = new List<Row>();
    const string improvedIntentPromptString = "You are a travel support intent classifier. Reply with exactly one uppercase label: " + "BOOKING, CANCELLATION, BAGGAGE, OTHER.\n" + "Examples: Change my reservation => BOOKING; Refund my cancelled flight => CANCELLATION; " + "What is my luggage allowance? => BAGGAGE; What's the weather? => OTHER. No extra text.";
    const string simpleIntentPromptString = "Classify the travel support intent into one of BOOKING, CANCELLATION, BAGGAGE, OTHER. Reply with one label.";
    foreach (IntentCase? itemIntentCase in intentCases)
    {
        resultRows.Add(
            await BenchmarkExperiment.EvaluateAsync(
                foundryToolRestClient,
                foundryOptions.ModelDeployment,
                "intent",
                "baseline",
                itemIntentCase.Id,
                itemIntentCase.Message,
                itemIntentCase.Expected,
                simpleIntentPromptString,
                null));
        resultRows.Add(
            await BenchmarkExperiment.EvaluateAsync(
                foundryToolRestClient,
                foundryOptions.ModelDeployment,
                "intent",
                "few-shot",
                itemIntentCase.Id,
                itemIntentCase.Message,
                itemIntentCase.Expected,
                improvedIntentPromptString,
                null));
        if (!string.IsNullOrWhiteSpace(fineTunedString))
        {
            resultRows.Add(
                await BenchmarkExperiment.EvaluateAsync(
                    foundryToolRestClient,
                    fineTunedString,
                    "intent",
                    "fine-tuned",
                    itemIntentCase.Id,
                    itemIntentCase.Message,
                    itemIntentCase.Expected,
                    simpleIntentPromptString,
                    null));
        }
    }

    foreach (FactCase? itemFactCase in factCases)
    {
        resultRows.Add(
            await BenchmarkExperiment.EvaluateAsync(
                foundryToolRestClient,
                foundryOptions.ModelDeployment,
                "policy facts",
                "baseline",
                itemFactCase.Id,
                itemFactCase.Question,
                itemFactCase.ExpectedFact,
                "Answer the travel customer question in one short sentence.",
                null));
        resultRows.Add(
            await BenchmarkExperiment.EvaluateAsync(
                foundryToolRestClient,
                foundryOptions.ModelDeployment,
                "policy facts",
                "grounding-instructions-only",
                itemFactCase.Id,
                itemFactCase.Question,
                itemFactCase.ExpectedFact,
                "Answer only when you have reliable information. Do not invent travel provider policies. " + "If information is unavailable, reply 'I don't know based on the supplied information.'",
                null));
        if (!string.IsNullOrWhiteSpace(storeIdString))
        {
            resultRows.Add(
                await BenchmarkExperiment.EvaluateAsync(
                    foundryToolRestClient,
                    foundryOptions.ModelDeployment,
                    "policy facts",
                    "file-search-RAG",
                    itemFactCase.Id,
                    itemFactCase.Question,
                    itemFactCase.ExpectedFact,
                    "Answer only using documents available through file_search. " + "When no source supports the answer, say you do not know. Cite sources when possible.",
                    storeIdString));
        }
    }

    // ------------------------------------------------------------------
    // RESULTS
    // ------------------------------------------------------------------
    // 1. Write the report and explain the observed results.
    var csvStringBuilder = new StringBuilder(
        "case_id,task,strategy,expected,output,correct,latency_ms,input_tokens,output_tokens,error\n");
    foreach (Row? row in resultRows)
    {
        csvStringBuilder.AppendLine(
            string.Join(
                ',',
                Csv(row.CaseId),
                Csv(row.Task),
                Csv(row.Strategy),
                Csv(row.Expected),
                Csv(row.Output),
                row.Correct ? "true" : "false",
                row.LatencyMs,
                row.InputTokens?.ToString() ?? "",
                row.OutputTokens?.ToString() ?? "",
                Csv(row.Error)));
    }

    await File.WriteAllTextAsync(
        outputString,
        csvStringBuilder.ToString());
    OutputPrinter.PrintReport(
        resultRows,
        outputString);
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}

string? Option(string nameString)
{
    int indexInt = Array.FindIndex(
        args,
        sString => sString.Equals(
            nameString,
            StringComparison.OrdinalIgnoreCase));
    if (indexInt < 0)
    {
        return null;
    }

    if (indexInt + 1 >= args.Length)
    {
        throw new ArgumentException($"{nameString} requires a value.");
    }

    return args[indexInt + 1];
}

static string Csv(string? valueString)
{
    return '"' + (valueString ?? "").Replace(
        "\"",
        "\"\"") + '"';
}
