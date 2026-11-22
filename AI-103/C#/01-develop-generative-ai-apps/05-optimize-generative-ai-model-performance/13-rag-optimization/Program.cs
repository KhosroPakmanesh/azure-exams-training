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
    if (args.Length == 0)
    {
        Console.WriteLine(
            "Pass --sample to index the included fictional travel policies, or pass your own documents/vector store ID.");
        return;
    }

    FoundryOptions foundryOptions = FoundryOptions.FromEnvironment();
    using var foundryToolRestClient = new FoundryToolRestClient(foundryOptions);
    bool sampleBool = args.Length == 1 && args[0].Equals(
        "--sample",
        StringComparison.OrdinalIgnoreCase);
    string[] documentArgStrings = sampleBool ? [Path.Combine(
        AppContext.BaseDirectory,
        "samples")] : args;
    string storeIdString = await FoundryVectorStoreSetup.ResolveAsync(
        foundryToolRestClient,
        documentArgStrings,
        "rag-optimization-lab");
    OutputPrinter.PrintStore(storeIdString);
    Profile[]? profiles = new[]
    {
        new Profile(
            "fast",
            3,
            "none",
            false,
            0.0),
        new Profile(
            "reranked",
            5,
            "auto",
            false,
            0.0),
        new Profile(
            "rewrite+threshold",
            5,
            "auto",
            true,
            0.2)
    };
    Question[]? questions = sampleBool ? new[]
    {
        new Question(
            "q01",
            "How many days before travel can I cancel a standard booking for a full refund?",
            "7 days",
            "cancellations.md"),
        new Question(
            "q02",
            "When should I arrive at the airport for an international departure?",
            "3 hours",
            "checkin.md"),
        new Question(
            "q03",
            "What does one additional checked bag cost?",
            "$45",
            "baggage.md"),
        new Question(
            "q04",
            "How long does a refund typically take to be processed?",
            "5 business days",
            "cancellations.md"),
        new Question(
            "q05",
            "What is the standard checked baggage weight limit?",
            "23 kg",
            "baggage.md")
    }

    : Array.Empty<Question>();

    // ------------------------------------------------------------------
    // RETRIEVAL AND ANSWERS
    // ------------------------------------------------------------------
    // 1. Compare retrieval profiles on each question.
    var rowResults = new List<Result>();
    if (sampleBool)
    {
        foreach (Question? question in questions)
        {
            foreach (Profile? profile in profiles)
            {
                rowResults.Add(
                    await RetrievalExperiment.AskAsync(
                        foundryToolRestClient,
                        foundryOptions.ModelDeployment,
                        storeIdString,
                        profile,
                        question));
            }
        }
    }
    else
    {
        Console.WriteLine("Type questions about your documents. Press Ctrl+C or enter quit to stop.\n");
        while (true)
        {
            string? inputString = ConsoleChat.ReadPrompt();
            if (inputString is null || inputString.Equals(
                "quit",
                StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(inputString))
            {
                continue;
            }

            foreach (Profile? profile in profiles)
            {
                rowResults.Add(
                    await RetrievalExperiment.AskAsync(
                        foundryToolRestClient,
                        foundryOptions.ModelDeployment,
                        storeIdString,
                        profile,
                        new Question(
                            "interactive",
                            inputString,
                            null,
                            null)));
            }
        }
    }

    if (rowResults.Count > 0)
    {
        string pathString = "rag-optimization-results.csv";
        var csvStringBuilder = new StringBuilder(
            "question_id,profile,expected,answer,retrieval_hit,answer_contains_fact,chunks,latency_ms,error\n");
        foreach (Result? result in rowResults)
        {
            csvStringBuilder.AppendLine(
                string.Join(
                    ',',
                    Csv(result.QuestionId),
                    Csv(result.Profile),
                    Csv(result.Expected),
                    Csv(result.Answer),
                    result.RetrievalHit?.ToString() ?? "",
                    result.AnswerContainsFact?.ToString() ?? "",
                    result.Chunks,
                    result.LatencyMs,
                    Csv(result.Error)));
        }

        await File.WriteAllTextAsync(
            pathString,
            csvStringBuilder.ToString());
        Console.WriteLine($"Results saved: {Path.GetFullPath(pathString)}");
        if (sampleBool)
        {
            Console.WriteLine("\nSample-only accuracy (simple expected-phrase check, not a semantic grader):");
            foreach (IGrouping<string, Result>? groupStringResultIGrouping in rowResults.GroupBy(result => result.Profile))
            {
                Console.WriteLine(
                    $"  {groupStringResultIGrouping.Key,-20} retrieved expected file: {groupStringResultIGrouping.Count(result => result.RetrievalHit == true)}/{groupStringResultIGrouping.Count()}, " + $"answer contains expected phrase: {groupStringResultIGrouping.Count(result => result.AnswerContainsFact == true)}/{groupStringResultIGrouping.Count()}");
            }
        }
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}

static string Csv(string? valueString)
{
    return '"' + (valueString ?? "").Replace(
        "\"",
        "\"\"") + '"';
}
