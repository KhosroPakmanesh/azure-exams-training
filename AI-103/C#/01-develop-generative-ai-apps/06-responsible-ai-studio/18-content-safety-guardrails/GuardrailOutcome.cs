using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AzureExamsTraining.AI103.Shared;

internal static class GuardrailOutcome
{
    internal static string ClassifyResponse(JsonElement rootJsonElement)
    {
        if (rootJsonElement.TryGetProperty(
            "status",
            out JsonElement statusJsonElement) && statusJsonElement.ValueKind == JsonValueKind.String && statusJsonElement.GetString() == "incomplete" && rootJsonElement.TryGetProperty(
                "incomplete_details",
                out JsonElement detailsJsonElement) && detailsJsonElement.ValueKind == JsonValueKind.Object && detailsJsonElement.TryGetProperty(
                    "reason",
                    out JsonElement reasonJsonElement) && reasonJsonElement.GetString() == "content_filter")
        {
            return "incomplete_content_filter";
        }

        if (rootJsonElement.TryGetProperty(
            "output",
            out JsonElement outputJsonElement) && outputJsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement itemJsonElement in outputJsonElement.EnumerateArray())
            {
                if (itemJsonElement.TryGetProperty(
                    "content",
                    out JsonElement contentJsonElement) && contentJsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement partJsonElement in contentJsonElement.EnumerateArray())
                    {
                        if (partJsonElement.TryGetProperty(
                            "type",
                            out JsonElement typeJsonElement) && typeJsonElement.GetString() == "refusal")
                        {
                            return "structured_refusal";
                        }
                    }
                }
            }
        }

        return "response_returned"; // Not a safety judgment.
    }
}
