using AzureExamsTraining.AI103.Shared;
using System.Text.Json;

OutputPrinter.PrintIntroduction();
try
{
    // ------------------------------------------------------------------
    // PREPARATION
    // ------------------------------------------------------------------
    // 1. Load the lab inputs and configure the connection when a live run requires one.
    using var foundryToolRestClient = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    object[] toolObjects = [
        new
    {
        type = "function",
        name = "get_time",
        description = "Return the current local date and time from this application's host computer.",
        parameters = new
        {
            type = "object",
            properties = new Dictionary<string, object>(),
            required = Array.Empty<string>(),
            additionalProperties = false
        },
        strict = true
    }

    ];
    string? previousIdString = null;

    // ------------------------------------------------------------------
    // CHAT TURNS
    // ------------------------------------------------------------------
    // 1. Read a prompt, call the model, and show the answer before reading the next prompt.
    while (true)
    {
        string? promptString = ConsoleChat.ReadPrompt();
        if (promptString is null)
        {
            break;
        }

        if (promptString.Length == 0)
        {
            continue;
        }

        JsonDocument? responseJsonDocument = null;
        try
        {
            responseJsonDocument = await foundryToolRestClient.CreateResponseAsync(
                promptString,
                toolObjects,
                previousIdString);
            // Function calls are requests, not executable code; C# runs only get_time.
            // A round limit prevents an unexpected model/tool loop.
            for (int roundInt = 0; roundInt < 8; roundInt++)
            {
                JsonElement[]? callJsonElements = responseJsonDocument.RootElement.GetProperty("output").EnumerateArray().Where(
                    itemJsonElement => itemJsonElement.TryGetProperty(
                        "type",
                        out JsonElement kindJsonElement) && kindJsonElement.GetString() == "function_call").ToArray();
                if (callJsonElements.Length == 0)
                {
                    break;
                }

                if (roundInt == 7)
                {
                    throw new InvalidOperationException("Too many consecutive function-call rounds.");
                }

                var resultObjects = new List<object>();
                foreach (JsonElement callJsonElement in callJsonElements)
                {
                    string? nameString = callJsonElement.GetProperty("name").GetString();
                    if (nameString != "get_time")
                    {
                        throw new InvalidOperationException($"Unexpected function requested: {nameString}");
                    }

                    string? argumentsTextString = callJsonElement.GetProperty("arguments").GetString() ?? "{}";
                    using JsonDocument? argumentsJsonDocument = JsonDocument.Parse(argumentsTextString);
                    if (argumentsJsonDocument.RootElement.ValueKind != JsonValueKind.Object || argumentsJsonDocument.RootElement.EnumerateObject().Any())
                    {
                        throw new InvalidOperationException("get_time takes no arguments.");
                    }

                    Console.WriteLine("[Executing approved local function: get_time]");
                    resultObjects.Add(
                        new
                        {
                            type = "function_call_output",
                            call_id = callJsonElement.GetProperty("call_id").GetString(),
                            output = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")
                        });
                }

                string? responseIdString = FoundryToolRestClient.RequireId(responseJsonDocument);
                JsonDocument? nextJsonDocument = await foundryToolRestClient.CreateResponseAsync(
                    resultObjects,
                    toolObjects,
                    responseIdString);
                responseJsonDocument.Dispose();
                responseJsonDocument = nextJsonDocument;
            }

            OutputPrinter.PrintAnswer(responseJsonDocument);
            previousIdString = FoundryToolRestClient.RequireId(responseJsonDocument);
        }
        catch (Exception exception)
        {
            ConsoleChat.ShowError(exception);
        }
        finally
        {
            responseJsonDocument?.Dispose();
        }
    }
}
catch (Exception exception)
{
    ConsoleChat.ShowError(exception);
    Environment.ExitCode = 1;
}
