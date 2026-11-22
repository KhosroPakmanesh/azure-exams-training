using System.Text.Json;

internal static class IncidentSequence
{
    internal static async Task<IncidentEvent[]> ReadAsync(string inputString)
    {
        IncidentEvent[] eventIncidentEvents = JsonSerializer.Deserialize<IncidentEvent[]>(
            await File.ReadAllTextAsync(inputString),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("No events provided.");
        if (eventIncidentEvents.Length == 0 || eventIncidentEvents.Select(incidentEvent => incidentEvent.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != eventIncidentEvents.Length)
        {
            throw new InvalidDataException("Events must not be empty and must have unique IDs.");
        }

        foreach (IncidentEvent incidentEvent in eventIncidentEvents)
        {
            if (string.IsNullOrWhiteSpace(incidentEvent.Id) || string.IsNullOrWhiteSpace(incidentEvent.Kind) || string.IsNullOrWhiteSpace(incidentEvent.Summary) || incidentEvent.Severity is < 1 or > 5 || !DateTimeOffset.TryParse(
                incidentEvent.Timestamp,
                out _))
            {
                throw new InvalidDataException(
                    $"Invalid event {incidentEvent.Id}: require a timestamp, kind, summary, and severity 1-5.");
            }

            if (incidentEvent.Kind is not ("feedback" or "verified_harm" or "abuse" or "release_approval"))
            {
                throw new InvalidDataException($"Unsupported event kind in {incidentEvent.Id}: {incidentEvent.Kind}.");
            }
        }

        return eventIncidentEvents
            .OrderBy(incidentEvent => DateTimeOffset.Parse(incidentEvent.Timestamp))
            .ToArray();
    }
}
