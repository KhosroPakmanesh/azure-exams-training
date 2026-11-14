namespace FoundryLabs.Shared;

/// <summary>
/// Loads configuration from src/Shared.FoundryClient/.env when running from the
/// source tree. This does not mutate process environment variables. Explicit
/// OS environment variables take precedence over values from the .env file.
/// </summary>
internal static class FoundryEnvFile
{
    public static IReadOnlyDictionary<string, string> Load()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var path = FindPath();
        if (path is null)
            return values;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim().TrimStart('\uFEFF');
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.Ordinal))
                line = line[7..].TrimStart();

            var equals = line.IndexOf('=');
            if (equals <= 0)
                continue;

            var name = line[..equals].Trim();
            if (name.Length == 0)
                continue;

            var value = line[(equals + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }
            else
            {
                // Inline comments are allowed after a space; '#' within a value is kept.
                for (var i = 1; i < value.Length; i++)
                {
                    if (value[i] == '#' && char.IsWhiteSpace(value[i - 1]))
                    {
                        value = value[..i].TrimEnd();
                        break;
                    }
                }
            }
            values[name] = value;
        }
        return values;
    }

    private static string? FindPath()
    {
        // Works with dotnet run, Visual Studio F5 and the local development API.
        foreach (var root in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (DirectoryInfo? dir = new(root); dir is not null; dir = dir.Parent)
            {
                var inSolution = Path.Combine(dir.FullName, "src", "Shared.FoundryClient", ".env");
                if (File.Exists(inSolution))
                    return inSolution;

                if (dir.Name.Equals("Shared.FoundryClient", StringComparison.OrdinalIgnoreCase))
                {
                    var inShared = Path.Combine(dir.FullName, ".env");
                    if (File.Exists(inShared))
                        return inShared;
                }
            }
        }
        return null;
    }
}
