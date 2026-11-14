namespace FoundryLabs.Shared;

/// <summary>
/// Shared Foundry configuration. Explicit process environment variables take
/// precedence over src/Shared.FoundryClient/.env when both are present.
/// </summary>
public sealed class FoundryOptions
{
    public required Uri Endpoint { get; init; }
    public required string ModelDeployment { get; init; }
    public string? ApiKey { get; init; }
    public string Instructions { get; init; } = DefaultInstructions;

    private const string DefaultInstructions =
        "You are a helpful AI assistant that answers questions and provides information.";

    public static FoundryOptions FromEnvironment()
    {
        var fileSettings = FoundryEnvFile.Load();
        string? Setting(string name)
        {
            var environment = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(environment))
                return environment;
            var fromFile = fileSettings.GetValueOrDefault(name);
            return string.IsNullOrWhiteSpace(fromFile) ? null : fromFile;
        }

        var endpointText = Setting("AZURE_OPENAI_ENDPOINT");
        var model = Setting("MODEL_DEPLOYMENT") ?? Setting("AZURE_OPENAI_DEPLOYMENT");
        if (string.IsNullOrWhiteSpace(endpointText))
            throw new InvalidOperationException(
                "Fill AZURE_OPENAI_ENDPOINT in src/Shared.FoundryClient/.env (Azure OpenAI resource endpoint, not the Foundry project endpoint).");
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException(
                "Fill MODEL_DEPLOYMENT in src/Shared.FoundryClient/.env with your deployed model name.");
        if (!Uri.TryCreate(endpointText.Trim(), UriKind.Absolute, out var original)
            || original.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT must be an absolute HTTPS URL.");
        if (original.AbsolutePath.Contains("/api/projects", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Use the Azure OpenAI resource endpoint, not the Foundry project endpoint.");

        // Accept either https://resource.openai.azure.com or its /openai/v1/ URL.
        var baseUrl = original.GetLeftPart(UriPartial.Authority);
        var path = original.AbsolutePath.TrimEnd('/');
        if (!string.IsNullOrEmpty(path) && !path.Equals("/openai/v1", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The endpoint path must be empty or /openai/v1/.");

        var instructions = Setting("SYSTEM_INSTRUCTIONS");
        return new FoundryOptions
        {
            Endpoint = new Uri(baseUrl + "/openai/v1/"),
            ModelDeployment = model.Trim(),
            ApiKey = Setting("AZURE_OPENAI_API_KEY"),
            Instructions = string.IsNullOrWhiteSpace(instructions) ? DefaultInstructions : instructions
        };
    }
}
