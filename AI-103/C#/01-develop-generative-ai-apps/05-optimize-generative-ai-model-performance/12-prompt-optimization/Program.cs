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
    string datasetPathString = Option("--dataset") ?? Path.Combine(
        AppContext.BaseDirectory,
        "samples",
        "evaluation.json");
    string outputPathString = Option("--output") ?? "prompt-optimization-results.csv";
    TestCase[] caseTestCases = JsonSerializer.Deserialize<TestCase[]>(
        await File.ReadAllTextAsync(datasetPathString),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("The evaluation file is empty.");
    if (caseTestCases.Length == 0 || caseTestCases.Any(
        testCase => string.IsNullOrWhiteSpace(testCase.Id) || string.IsNullOrWhiteSpace(testCase.Message) || !Labels.Contains(testCase.Expected)))
    {
        throw new InvalidDataException("Each case needs an id, a message, and a valid expected label.");
    }

    const string fewShotInstructionsString = "You are a travel support intent classifier. Return exactly one label: " + "BOOKING, CANCELLATION, BAGGAGE, or OTHER.\nExamples:\n" + "Customer: I need to move my flight to Friday.\nLabel: BOOKING\n" + "Customer: Please cancel the reservation and refund me.\nLabel: CANCELLATION\n" + "Customer: Is my suitcase too heavy?\nLabel: BAGGAGE\n" + "Customer: What is the weather in Paris?\nLabel: OTHER";
    Variant[]? variants = new[]
    {
        new Variant(
            "zero-shot",
            "Classify the travel customer message as BOOKING, CANCELLATION, BAGGAGE, or OTHER. Reply with one label.",
            null),
        new Variant(
            "system-prompt",
            "You are a travel support intent classifier. Return exactly one uppercase label: " + "BOOKING (new/change a reservation), CANCELLATION (cancel/refund), " + "BAGGAGE (bags/weight/allowances), or OTHER. Do not explain your answer.",
            null),
        new Variant(
            "few-shot",
            fewShotInstructionsString,
            null),
        new Variant(
            "few-shot-temp-0",
            fewShotInstructionsString,
            0.0)
    };
    if (args.Contains(
        "--preview",
        StringComparer.OrdinalIgnoreCase))
    {
        OutputPrinter.PrintPreview(
            caseTestCases,
            variants);
        return;
    }

    // ------------------------------------------------------------------
    // PROMPT COMPARISON
    // ------------------------------------------------------------------
    // 1. Use the same cases for each prompt variant.
    FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
    using var foundryToolRestClient = new FoundryToolRestClient(foundryOptions);
    var rowResults = new List<Result>();
    foreach (Variant? variant in variants)
    {
        Console.WriteLine($"\n{variant.Name} ({caseTestCases.Length} requests):");
        foreach (TestCase? itemTestCase in caseTestCases)
        {
            Stopwatch? stopwatch = Stopwatch.StartNew();
            try
            {
                var bodyStringObjectDictionary = new Dictionary<string, object?>
                {
                    ["model"] = foundryOptions.ModelDeployment,
                    ["instructions"] = variant.Instructions,
                    ["input"] = itemTestCase.Message,
                    ["store"] = false
                };
                if (variant.Temperature is double temperatureDouble)
                {
                    bodyStringObjectDictionary["temperature"] = temperatureDouble;
                }

                using JsonDocument responseJsonDocument = await foundryToolRestClient.PostJsonAsync(
                    "responses",
                    bodyStringObjectDictionary);
                stopwatch.Stop();
                string rawString = FoundryToolRestClient.GetOutputText(responseJsonDocument).Trim();
                string actualString = Normalize(rawString);
                JsonElement usageJsonElement = responseJsonDocument.RootElement.TryGetProperty(
                    "usage",
                    out JsonElement valueJsonElement) ? valueJsonElement : default;
                var rowResult = new Result(
                    variant.Name,
                    itemTestCase.Id,
                    itemTestCase.Expected,
                    actualString,
                    rawString,
                    actualString == itemTestCase.Expected,
                    stopwatch.ElapsedMilliseconds,
                    Tokens(
                        usageJsonElement,
                        "input_tokens"),
                    Tokens(
                        usageJsonElement,
                        "output_tokens"),
                    "");
                rowResults.Add(rowResult);
                Console.WriteLine(
                    $"  {itemTestCase.Id,-10} {(rowResult.Correct ? "PASS" : "FAIL")} expected={itemTestCase.Expected} actual={actualString}");
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                rowResults.Add(
                    new Result(
                        variant.Name,
                        itemTestCase.Id,
                        itemTestCase.Expected,
                        "",
                        "",
                        false,
                        stopwatch.ElapsedMilliseconds,
                        null,
                        null,
                        exception.Message));
                Console.WriteLine($"  {itemTestCase.Id,-10} ERROR: {exception.Message}");
            }
        }
    }

    // ------------------------------------------------------------------
    // RESULTS
    // ------------------------------------------------------------------
    // 1. Write the report and explain the observed results.
    var csvStringBuilder = new StringBuilder("variant,id,expected,predicted,raw,correct,latency_ms,input_tokens,output_tokens,error\n");
    foreach (Result? result in rowResults)
    {
        csvStringBuilder.AppendLine(
            string.Join(
                ',',
                Csv(result.Variant),
                Csv(result.Id),
                Csv(result.Expected),
                Csv(result.Predicted),
                Csv(result.Raw),
                result.Correct ? "true" : "false",
                result.LatencyMs,
                result.InputTokens?.ToString() ?? "",
                result.OutputTokens?.ToString() ?? "",
                Csv(result.Error)));
    }

    await File.WriteAllTextAsync(
        outputPathString,
        csvStringBuilder.ToString());
    OutputPrinter.PrintReport(
        rowResults,
        outputPathString);
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
        aString => aString.Equals(
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

static string Normalize(string valueString)
{
    // Deliberately strict: a response containing extra explanation is not format-compliant.
    return valueString.Trim().ToUpperInvariant();
}

static int? Tokens(
    JsonElement usageJsonElement,
    string keyString)
{
    return usageJsonElement.ValueKind == JsonValueKind.Object && usageJsonElement.TryGetProperty(
        keyString,
        out JsonElement vJsonElement) && vJsonElement.TryGetInt32(out int nInt) ? nInt : null;
}

static string Csv(string? valueString)
{
    return '"' + (valueString ?? "").Replace(
        "\"",
        "\"\"") + '"';
}
