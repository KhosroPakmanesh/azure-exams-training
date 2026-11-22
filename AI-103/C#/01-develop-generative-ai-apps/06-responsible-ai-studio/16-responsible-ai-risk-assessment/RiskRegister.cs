using System.Text;
using System.Text.Json;

internal static class RiskRegister
{
    internal static async Task<Risk[]> ReadAsync(string inputString)
    {
        Risk[] risks = JsonSerializer.Deserialize<Risk[]>(
            await File.ReadAllTextAsync(inputString),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("The risk register could not be read.");
        if (risks.Length == 0 || risks.Select(risk => risk.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != risks.Length)
        {
            throw new InvalidDataException("Risks must have unique IDs and the register cannot be empty.");
        }

        foreach (Risk risk in risks)
        {
            if (string.IsNullOrWhiteSpace(risk.Id) ||
                string.IsNullOrWhiteSpace(risk.Harm) ||
                string.IsNullOrWhiteSpace(risk.Scenario) ||
                string.IsNullOrWhiteSpace(risk.Owner) ||
                string.IsNullOrWhiteSpace(risk.Mitigation) ||
                risk.Likelihood is < 1 or > 5 ||
                risk.Impact is < 1 or > 5)
            {
                throw new InvalidDataException($"Invalid risk {risk.Id}: required fields and 1-5 scores are mandatory.");
            }
        }

        return risks;
    }

    internal static RankedRisk[] Rank(IEnumerable<Risk> risks)
    {
        var rankedRisks = new List<RankedRisk>();
        foreach (Risk risk in risks)
        {
            int scoreInt = risk.Likelihood * risk.Impact;
            string priorityString = scoreInt switch
            {
                >= 15 => "HIGH",
                >= 8 => "MEDIUM",
                _ => "LOW"
            };
            rankedRisks.Add(
                new RankedRisk(
                    risk,
                    scoreInt,
                    priorityString));
        }

        return rankedRisks
            .OrderByDescending(rankedRisk => rankedRisk.Score)
            .ThenBy(
                rankedRisk => rankedRisk.Risk.Id,
                StringComparer.Ordinal)
            .ToArray();
    }

    internal static async Task WriteCsvAsync(
        RankedRisk[] rankedRisks,
        string outputString)
    {
        var csvStringBuilder = new StringBuilder(
            "id,category,scenario,harm,likelihood,impact,score,priority,owner,mitigation,evidence,status\n");
        foreach (RankedRisk itemRankedRisk in rankedRisks)
        {
            Risk risk = itemRankedRisk.Risk;
            csvStringBuilder.AppendLine(
                string.Join(
                    ",",
                    Csv(risk.Id),
                    Csv(risk.Category),
                    Csv(risk.Scenario),
                    Csv(risk.Harm),
                    risk.Likelihood.ToString(),
                    risk.Impact.ToString(),
                    itemRankedRisk.Score.ToString(),
                    Csv(itemRankedRisk.Priority),
                    Csv(risk.Owner),
                    Csv(risk.Mitigation),
                    Csv(risk.Evidence),
                    Csv(risk.Status)));
        }

        await File.WriteAllTextAsync(
            outputString,
            csvStringBuilder.ToString());
    }

    private static string Csv(string? textString)
    {
        return '"' + (textString ?? "").Replace(
            "\"",
            "\"\"") + '"';
    }
}
