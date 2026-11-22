using System.Text.Json;
using AzureExamsTraining.AI103.FineTuning;

internal static class TrainingDataset
{
    internal static async Task WriteSamplesAsync(
        string pathString,
        IReadOnlyDictionary<string, string[]> groupsStringStringsIReadOnlyDictionary,
        int startInt,
        int countInt)
    {
        var lineStrings = new List<string>();
        foreach (var (labelString, phraseStrings) in groupsStringStringsIReadOnlyDictionary)
        {
            foreach (string? phraseString in phraseStrings.Skip(startInt).Take(countInt))
            {
                lineStrings.Add(
                    JsonSerializer.Serialize(
                        new
                        {
                            messages = new[] {
                                new {
                                    role = "system",
                                    content = SamplePhrases.Instructions
                                },
                                new {
                                    role = "user",
                                    content = phraseString
                                },
                                new {
                                    role = "assistant",
                                    content = labelString
                                }
                            }
                        }));
            }
        }

        await File.WriteAllLinesAsync(
            pathString,
            lineStrings);
    }

    internal static List<Example> Validate(string pathString)
    {
        if (!File.Exists(pathString))
        {
            throw new FileNotFoundException(
                "JSONL file not found",
                pathString);
        }

        var examples = new List<Example>();
        var observedStrings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int lineNumberInt = 0;
        foreach (string lineString in File.ReadLines(pathString))
        {
            lineNumberInt++;
            if (string.IsNullOrWhiteSpace(lineString))
            {
                throw new InvalidDataException($"{pathString}:{lineNumberInt}: blank JSONL line.");
            }

            try
            {
                using JsonDocument documentJsonDocument = JsonDocument.Parse(lineString);
                JsonElement rootJsonElement = documentJsonDocument.RootElement;
                if (!rootJsonElement.TryGetProperty(
                    "messages",
                    out JsonElement messagesJsonElement) || messagesJsonElement.ValueKind != JsonValueKind.Array || messagesJsonElement.GetArrayLength() != 3)
                {
                    throw new InvalidDataException("Expected exactly three messages (system, user, assistant).");
                }

                string[] roleStrings = ["system", "user", "assistant"];
                string[]? contentStrings = new string[3];
                for (int iInt = 0; iInt < 3; iInt++)
                {
                    JsonElement messageJsonElement = messagesJsonElement[iInt];
                    if (StringField(
                        messageJsonElement,
                        "role") != roleStrings[iInt])
                    {
                        throw new InvalidDataException($"Message {iInt + 1} must have role {roleStrings[iInt]}.");
                    }

                    contentStrings[iInt] = StringField(
                        messageJsonElement,
                        "content") ?? "";
                    if (string.IsNullOrWhiteSpace(contentStrings[iInt]))
                    {
                        throw new InvalidDataException($"Message {iInt + 1} requires content.");
                    }
                }

                if (!SamplePhrases.All.ContainsKey(contentStrings[2]))
                {
                    throw new InvalidDataException("Assistant label must be BOOKING, CANCELLATION, BAGGAGE, or OTHER.");
                }

                if (!observedStrings.Add(contentStrings[1]))
                {
                    throw new InvalidDataException("Duplicate user message in file.");
                }

                examples.Add(new Example(
                    contentStrings[1],
                    contentStrings[2]));
            }
            catch (JsonException jsonException)
            {
                throw new InvalidDataException(
                    $"{pathString}:{lineNumberInt}: invalid JSON: {jsonException.Message}",
                    jsonException);
            }
            catch (InvalidDataException invalidDataException)
            {
                throw new InvalidDataException(
                    $"{pathString}:{lineNumberInt}: {invalidDataException.Message}",
                    invalidDataException);
            }
        }

        if (examples.Count == 0)
        {
            throw new InvalidDataException($"{pathString}: JSONL has no examples.");
        }

        return examples;
    }

    internal static string? StringField(
        JsonElement eJsonElement,
        string fieldString)
    {
        return eJsonElement.ValueKind == JsonValueKind.Object && eJsonElement.TryGetProperty(
            fieldString,
            out JsonElement valueJsonElement) && valueJsonElement.ValueKind == JsonValueKind.String ? valueJsonElement.GetString() : null;
    }
}
