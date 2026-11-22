using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class BenchmarkExperiment
{
    internal static async Task<Row> EvaluateAsync(
        FoundryToolRestClient foundryToolRestClient,
        string modelString,
        string taskString,
        string strategyString,
        string idString,
        string inputString,
        string expectedString,
        string instructionsString,
        string? storeIdString)
    {
        Stopwatch? stopwatch = Stopwatch.StartNew();
        try
        {
            using JsonDocument responseJsonDocument = storeIdString is null ? await foundryToolRestClient.PostJsonAsync(
                "responses",
                new
                {
                    model = modelString,
                    instructions = instructionsString,
                    input = inputString,
                    store = false
                }) : await foundryToolRestClient.CreateResponseAsync(
                    inputString,
                    [new {
                        type = "file_search",
                        vector_store_ids = new[] { storeIdString }
                    }],
                    instructions: instructionsString);
            stopwatch.Stop();
            string answerString = FoundryToolRestClient.GetOutputText(responseJsonDocument).Trim();
            bool correctBool = taskString == "intent" ? answerString.Equals(
                expectedString,
                StringComparison.OrdinalIgnoreCase) : answerString.Contains(
                    expectedString,
                    StringComparison.OrdinalIgnoreCase);
            JsonElement usageJsonElement = responseJsonDocument.RootElement.TryGetProperty(
                "usage",
                out JsonElement uJsonElement) ? uJsonElement : default;
            Console.WriteLine(
                $"{idString,-7} {strategyString,-28} {(correctBool ? "PASS" : "FAIL")} answer={Preview(
                    answerString,
                    90)}");
            return new Row(
                idString,
                taskString,
                strategyString,
                expectedString,
                answerString,
                correctBool,
                stopwatch.ElapsedMilliseconds,
                Tokens(
                    usageJsonElement,
                    "input_tokens"),
                Tokens(
                    usageJsonElement,
                    "output_tokens"),
                "");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            Console.WriteLine($"{idString,-7} {strategyString,-28} ERROR {exception.Message}");
            return new Row(
                idString,
                taskString,
                strategyString,
                expectedString,
                "",
                false,
                stopwatch.ElapsedMilliseconds,
                null,
                null,
                exception.Message);
        }
    }

    private static int? Tokens(
        JsonElement usageJsonElement,
        string nameString)
    {
        return usageJsonElement.ValueKind == JsonValueKind.Object && usageJsonElement.TryGetProperty(
            nameString,
            out JsonElement valueJsonElement) && valueJsonElement.TryGetInt32(out int nInt) ? nInt : null;
    }

    private static string Preview(
        string textString,
        int maxInt)
    {
        return textString.Length <= maxInt ? textString : textString[..maxInt] + "...";
    }
}
