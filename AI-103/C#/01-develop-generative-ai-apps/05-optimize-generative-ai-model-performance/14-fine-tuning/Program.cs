using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AzureExamsTraining.AI103.Shared;
using AzureExamsTraining.AI103.FineTuning;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    if (args.Length == 0)
    {
        return;
    }

    string commandString = args[0].ToLowerInvariant();

    // ------------------------------------------------------------------
    // DATA AND JOB OPERATIONS
    // ------------------------------------------------------------------
    // 1. Run the requested local or Azure operation.
    switch (commandString)
    {
        case "prepare":
            {
                string folderString = args.Length > 1 ? args[1] : "fine-tuning-data";
                Directory.CreateDirectory(folderString);
                IReadOnlyDictionary<string, string[]>? groupsStringStringsIReadOnlyDictionary = SamplePhrases.All;
                await TrainingDataset.WriteSamplesAsync(
                    Path.Combine(
                        folderString,
                        "train.jsonl"),
                    groupsStringStringsIReadOnlyDictionary,
                    0,
                    10);
                await TrainingDataset.WriteSamplesAsync(
                    Path.Combine(
                        folderString,
                        "validation.jsonl"),
                    groupsStringStringsIReadOnlyDictionary,
                    10,
                    3);
                await TrainingDataset.WriteSamplesAsync(
                    Path.Combine(
                        folderString,
                        "test.jsonl"),
                    groupsStringStringsIReadOnlyDictionary,
                    13,
                    3);
                OutputPrinter.PrintPrepared(folderString);
                break;
            }

        case "validate":
            {
                if (args.Length < 2)
                {
                    throw new ArgumentException("Provide at least one JSONL path.");
                }

                foreach (string pathString in args.Skip(1))
                {
                    Console.WriteLine($"{pathString}: {TrainingDataset.Validate(pathString).Count} valid examples.");
                }

                break;
            }

        case "upload":
            {
                Require(
                    2,
                    "upload <validated-file.jsonl>");
                Console.WriteLine($"Validated {TrainingDataset.Validate(args[1]).Count} examples.");
                FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
                using var uploaderTrainingFileUploader = new TrainingFileUploader(foundryOptions);
                using JsonDocument uploadJsonDocument = await uploaderTrainingFileUploader.UploadAsync(args[1]);
                Console.WriteLine($"Uploaded training file ID: {FoundryToolRestClient.RequireId(uploadJsonDocument)}");
                Console.WriteLine(
                    "Save this ID for create-job. Uploaded files remain in your Azure resource until deleted.");
                break;
            }

        case "create-job":
            {
                Require(
                    4,
                    "create-job <train-file-id> <validation-file-id> <base-model> --confirm-costs");
                if (!args.Skip(4).Contains(
                    "--confirm-costs",
                    StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "No training job started. Add --confirm-costs after reviewing charges and model eligibility.");
                }

                if (!args[1].StartsWith(
                    "file-",
                    StringComparison.OrdinalIgnoreCase) || !args[2].StartsWith(
                        "file-",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Both training and validation arguments must be uploaded file IDs (file-...).");
                }

                using var foundryToolRestClient = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
                using JsonDocument jobJsonDocument = await foundryToolRestClient.PostJsonAsync(
                    "fine_tuning/jobs",
                    new
                    {
                        model = args[3],
                        training_file = args[1],
                        validation_file = args[2]
                    });
                Console.WriteLine($"Submitted fine-tuning job: {FoundryToolRestClient.RequireId(jobJsonDocument)}");
                Console.WriteLine("This starts a potentially billable job. Check its status before deploying the result.");
                break;
            }

        case "status":
            {
                Require(
                    2,
                    "status <fine-tuning-job-id>");
                using var foundryToolRestClient = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
                using JsonDocument responseJsonDocument = await foundryToolRestClient.GetAsync($"fine_tuning/jobs/{Uri.EscapeDataString(args[1])}");
                JsonElement rootJsonElement = responseJsonDocument.RootElement;
                Console.WriteLine($"Job: {TrainingDataset.StringField(
                    rootJsonElement,
                    "id") ?? "(unknown)"}");
                Console.WriteLine($"Status: {TrainingDataset.StringField(
                    rootJsonElement,
                    "status") ?? "(unknown)"}");
                Console.WriteLine(
                    $"Fine-tuned model: {TrainingDataset.StringField(
                        rootJsonElement,
                        "fine_tuned_model") ?? "not yet available"}");
                Console.WriteLine("A successful job does not automatically make the model available at a deployment name.");
                break;
            }

        case "compare":
            {
                Require(
                    3,
                    "compare <deployed-fine-tuned-model> <test.jsonl> [results.csv]");
                string deploymentString = args[1];
                List<Example>? sampleExamples = TrainingDataset.Validate(args[2]);
                string csvPathString = args.Length > 3 ? args[3] : "fine-tuning-comparison.csv";
                FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
                using var foundryToolRestClient = new FoundryToolRestClient(foundryOptions);
                var csvStringBuilder = new StringBuilder("case,model,expected,predicted,correct,latency_ms,input_tokens,output_tokens,error\n");
                for (int iInt = 0; iInt < sampleExamples.Count; iInt++)
                {
                    foreach (var (nameString, modelString) in new[]
                    {
                    ("baseline", foundryOptions.ModelDeployment),
                    ("fine-tuned", deploymentString)
                }

                    )
                    {
                        Stopwatch? stopwatch = Stopwatch.StartNew();
                        string actualString = "", errorString = "";
                        int? inputTokensInt = null, outputTokensInt = null;
                        try
                        {
                            using JsonDocument responseJsonDocument = await foundryToolRestClient.PostJsonAsync(
                                "responses",
                                new
                                {
                                    model = modelString,
                                    instructions = SamplePhrases.Instructions,
                                    input = sampleExamples[iInt].Message,
                                    store = false
                                });
                            actualString = FoundryToolRestClient.GetOutputText(responseJsonDocument).Trim().ToUpperInvariant();
                            if (responseJsonDocument.RootElement.TryGetProperty(
                                "usage",
                                out JsonElement usageJsonElement))
                            {
                                inputTokensInt = Tokens(
                                    usageJsonElement,
                                    "input_tokens");
                                outputTokensInt = Tokens(
                                    usageJsonElement,
                                    "output_tokens");
                            }
                        }
                        catch (Exception exception)
                        {
                            errorString = exception.Message;
                        }

                        stopwatch.Stop();
                        bool correctBool = errorString.Length == 0 && actualString == sampleExamples[iInt].Label;
                        csvStringBuilder.AppendLine(
                            string.Join(
                                ',',
                                iInt + 1,
                                Csv(nameString),
                                Csv(sampleExamples[iInt].Label),
                                Csv(actualString),
                                correctBool ? "true" : "false",
                                stopwatch.ElapsedMilliseconds,
                                inputTokensInt?.ToString() ?? "",
                                outputTokensInt?.ToString() ?? "",
                                Csv(errorString)));
                        Console.WriteLine(
                            $"{iInt + 1,2} {nameString,-12} expected={sampleExamples[iInt].Label,-12} actual={actualString,-12} " + $"{(errorString.Length > 0 ? "ERROR " + errorString : correctBool ? "PASS" : "FAIL")}");
                    }
                }

                await File.WriteAllTextAsync(
                    csvPathString,
                    csvStringBuilder.ToString());
                Console.WriteLine($"Comparison saved: {Path.GetFullPath(csvPathString)}");
                break;
            }

        default:
            throw new ArgumentException($"Unknown command: {args[0]}");
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}

void Require(
    int countInt,
    string usageString)
{
    if (args.Length < countInt)
    {
        throw new ArgumentException($"Usage: {usageString}");
    }
}

static int? Tokens(
    JsonElement usageJsonElement,
    string fieldString)
{
    return usageJsonElement.ValueKind == JsonValueKind.Object && usageJsonElement.TryGetProperty(
        fieldString,
        out JsonElement xJsonElement) && xJsonElement.TryGetInt32(out int nInt) ? nInt : null;
}

static string Csv(string? valueString)
{
    return '"' + (valueString ?? "").Replace(
        "\"",
        "\"\"") + '"';
}
