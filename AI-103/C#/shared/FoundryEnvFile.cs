namespace AzureExamsTraining.AI103.Shared;
/// <summary>
/// Loads configuration from C#/shared/.env when running from the
/// source tree. This does not mutate process environment variables. Explicit
/// OS environment variables take precedence over values from the .env file.
/// </summary>
internal static class FoundryEnvFile
{
    public static IReadOnlyDictionary<string, string> Load()
    {
        var valuesStringStringDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? pathString = FindPath();
        if (pathString is null)
        {
            return valuesStringStringDictionary;
        }

        foreach (string? rawString in File.ReadLines(pathString))
        {
            string? lineString = rawString.Trim().TrimStart('\uFEFF');
            if (lineString.Length == 0 || lineString.StartsWith('#'))
            {
                continue;
            }

            if (lineString.StartsWith(
                "export ",
                StringComparison.Ordinal))
            {
                lineString = lineString[7..].TrimStart();
            }

            int equalsInt = lineString.IndexOf('=');
            if (equalsInt <= 0)
            {
                continue;
            }

            string? nameString = lineString[..equalsInt].Trim();
            if (nameString.Length == 0)
            {
                continue;
            }

            string? valueString = lineString[(equalsInt + 1)..].Trim();
            if (valueString.Length >= 2 && ((valueString[0] == '"' && valueString[^1] == '"') || (valueString[0] == '\'' && valueString[^1] == '\'')))
            {
                valueString = valueString[1..^1];
            }
            else
            {
                // Inline comments are allowed after a space; '#' within a value is kept.
                for (int iInt = 1; iInt < valueString.Length; iInt++)
                {
                    if (valueString[iInt] == '#' && char.IsWhiteSpace(valueString[iInt - 1]))
                    {
                        valueString = valueString[..iInt].TrimEnd();
                        break;
                    }
                }
            }

            valuesStringStringDictionary[nameString] = valueString;
        }

        return valuesStringStringDictionary;
    }

    private static string? FindPath()
    {
        // Works with dotnet run, Visual Studio F5 and the local development API.
        foreach (string? rootString in new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        }

        )
        {
            for (DirectoryInfo? dirDirectoryInfo = new(rootString); dirDirectoryInfo is not null; dirDirectoryInfo = dirDirectoryInfo.Parent)
            {
                string? inSolutionString = Path.Combine(
                    dirDirectoryInfo.FullName,
                    "C#",
                    "shared",
                    ".env");
                if (File.Exists(inSolutionString))
                {
                    return inSolutionString;
                }

                if (dirDirectoryInfo.Name.Equals(
                    "shared",
                    StringComparison.OrdinalIgnoreCase) && dirDirectoryInfo.Parent?.Name == "C#")
                {
                    string? inSharedString = Path.Combine(
                        dirDirectoryInfo.FullName,
                        ".env");
                    if (File.Exists(inSharedString))
                    {
                        return inSharedString;
                    }
                }
            }
        }

        return null;
    }
}
