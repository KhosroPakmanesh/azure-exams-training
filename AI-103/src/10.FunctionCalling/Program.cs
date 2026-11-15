using FoundryLabs.Shared;
using System.Text.Json;

ConsoleChat.Banner("10: function tool (get_time)");
Console.WriteLine("Try: What is the current local time?\n");
try
{
    using var client = new FoundryToolRestClient(FoundryOptions.FromEnvironment());
    object[] tools =
    [
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
    string? previousId = null;

    while (true)
    {
        var prompt = ConsoleChat.ReadPrompt();
        if (prompt is null) break;
        if (prompt.Length == 0) continue;
        JsonDocument? response = null;
        try
        {
            response = await client.CreateResponseAsync(prompt, tools, previousId);
            // Function calls are requests, not executable code; C# runs only get_time.
            // A round limit prevents an unexpected model/tool loop.
            for (int round = 0; round < 8; round++)
            {
                var calls = response.RootElement.GetProperty("output").EnumerateArray()
                    .Where(item => item.TryGetProperty("type", out var kind) &&
                                   kind.GetString() == "function_call").ToArray();
                if (calls.Length == 0) break;
                if (round == 7)
                    throw new InvalidOperationException("Too many consecutive function-call rounds.");

                var results = new List<object>();
                foreach (var call in calls)
                {
                    var name = call.GetProperty("name").GetString();
                    if (name != "get_time")
                        throw new InvalidOperationException($"Unexpected function requested: {name}");
                    var argumentsText = call.GetProperty("arguments").GetString() ?? "{}";
                    using var arguments = JsonDocument.Parse(argumentsText);
                    if (arguments.RootElement.ValueKind != JsonValueKind.Object ||
                        arguments.RootElement.EnumerateObject().Any())
                        throw new InvalidOperationException("get_time takes no arguments.");
                    Console.WriteLine("[Executing approved local function: get_time]");
                    results.Add(new
                    {
                        type = "function_call_output",
                        call_id = call.GetProperty("call_id").GetString(),
                        output = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")
                    });
                }
                var responseId = FoundryToolRestClient.RequireId(response);
                var next = await client.CreateResponseAsync(results, tools, responseId);
                response.Dispose();
                response = next;
            }
            FoundryToolRestClient.PrintAnswer(response);
            previousId = FoundryToolRestClient.RequireId(response);
        }
        catch (Exception ex) { ConsoleChat.ShowError(ex); }
        finally { response?.Dispose(); }
    }
}
catch (Exception ex) { ConsoleChat.ShowError(ex); Environment.ExitCode = 1; }
