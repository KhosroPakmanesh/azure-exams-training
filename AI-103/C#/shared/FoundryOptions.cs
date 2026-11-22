namespace AzureExamsTraining.AI103.Shared;
/// <summary>
/// Shared Foundry configuration. Explicit process environment variables take
/// precedence over C#/shared/.env when both are present.
/// </summary>
public sealed class FoundryOptions
{
    public required Uri Endpoint
    {
        get; init;
    }
    public required string ModelDeployment
    {
        get; init;
    }
    public string? ApiKey
    {
        get; init;
    }
    public string Instructions { get; init; } = DefaultInstructionsString;

    private const string DefaultInstructionsString = "You are a helpful AI assistant that answers questions and provides information.";
    public static FoundryOptions FromEnvironment()
    {
        IReadOnlyDictionary<string, string>? fileSettingsStringStringIReadOnlyDictionary = FoundryEnvFile.Load();
        string? Setting(string nameString)
        {
            string? environmentString = Environment.GetEnvironmentVariable(nameString);
            if (!string.IsNullOrWhiteSpace(environmentString))
            {
                return environmentString;
            }

            string? fromFileString = fileSettingsStringStringIReadOnlyDictionary.GetValueOrDefault(nameString);
            return string.IsNullOrWhiteSpace(fromFileString) ? null : fromFileString;
        }

        string? endpointTextString = Setting("AZURE_OPENAI_ENDPOINT");
        string? modelString = Setting("MODEL_DEPLOYMENT") ?? Setting("AZURE_OPENAI_DEPLOYMENT");
        if (string.IsNullOrWhiteSpace(endpointTextString))
        {
            throw new InvalidOperationException(
                "Fill AZURE_OPENAI_ENDPOINT in C#/shared/.env (Azure OpenAI resource endpoint, not the Foundry project endpoint).");
        }

        if (string.IsNullOrWhiteSpace(modelString))
        {
            throw new InvalidOperationException("Fill MODEL_DEPLOYMENT in C#/shared/.env with your deployed model name.");
        }

        if (!Uri.TryCreate(
            endpointTextString.Trim(),
            UriKind.Absolute,
            out Uri? originalUri) || originalUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT must be an absolute HTTPS URL.");
        }

        if (originalUri.AbsolutePath.Contains(
            "/api/projects",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Use the Azure OpenAI resource endpoint, not the Foundry project endpoint.");
        }

        // Accept either https://resource.openai.azure.com or its /openai/v1/ URL.
        string? baseUrlString = originalUri.GetLeftPart(UriPartial.Authority);
        string? pathString = originalUri.AbsolutePath.TrimEnd('/');
        if (!string.IsNullOrEmpty(pathString) && !pathString.Equals(
            "/openai/v1",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The endpoint path must be empty or /openai/v1/.");
        }

        string? instructionsString = Setting("SYSTEM_INSTRUCTIONS");
        return new FoundryOptions
        {
            Endpoint = new Uri(baseUrlString + "/openai/v1/"),
            ModelDeployment = modelString.Trim(),
            ApiKey = Setting("AZURE_OPENAI_API_KEY"),
            Instructions = string.IsNullOrWhiteSpace(instructionsString) ? DefaultInstructionsString : instructionsString
        };
    }
}
