namespace AzureExamsTraining.AI103.FineTuning;
// Fictional, non-sensitive examples. The examples teach format/intent behavior, not facts.
internal static class SamplePhrases
{
    internal const string Instructions = "Classify the travel support customer's message into exactly one uppercase label: " + "BOOKING, CANCELLATION, BAGGAGE, or OTHER. Do not add any explanation.";
    internal static readonly IReadOnlyDictionary<string, string[]> All = new Dictionary<string, string[]>
    {
        ["BOOKING"] = [
            "Book me a ticket to Oslo next Monday.",
            "Change my seat to the aisle on the outbound flight.",
            "I want to reserve two seats for Berlin.",
            "Move my departure from Tuesday to Friday.",
            "Can you help arrange a connecting flight?",
            "Add my spouse to the travel reservation.",
            "I need a new reservation for Amsterdam.",
            "Please upgrade my booking to business class.",
            "Can I book a return flight for Sunday?",
            "I want to change the arrival city on my ticket.",
            "Reserve three tickets to Lisbon next month.",
            "Change my travel date to the 14th.",
            "Could you confirm a seat for my partner?",
            "Book the earliest flight to Prague.",
            "I need to rebook my outbound trip.",
            "Please switch my ticket to a window seat."
        ],
        ["CANCELLATION"] = [
            "Cancel my reservation and issue a refund.",
            "How do I request money back for my cancelled flight?",
            "Please void both tickets for my trip.",
            "I need to cancel the entire itinerary.",
            "What is the refund status for my cancelled booking?",
            "I no longer wish to travel. Cancel my seat.",
            "Can you reverse the charges for my cancelled trip?",
            "Please begin the cancellation process.",
            "My travel plans fell through. Refund the ticket.",
            "I need a refund after the airline cancelled.",
            "Cancel my return leg and refund it.",
            "I want to withdraw the reservation.",
            "Can I claim a refund for an unused flight?",
            "Please cancel two reservations immediately.",
            "I want to know how to get a cancellation refund.",
            "My trip is off. Can you cancel it?"
        ],
        ["BAGGAGE"] = [
            "What is the checked luggage allowance?",
            "Can I bring a second suitcase on board?",
            "How much does an extra bag cost?",
            "My bag weighs 28 kilograms. Is that allowed?",
            "Do you charge for oversize luggage?",
            "Is my backpack considered a carry-on?",
            "Where can I find the luggage weight limit?",
            "I want to add a checked bag to my ticket.",
            "Can my skis travel as special baggage?",
            "What are the dimensions for cabin luggage?",
            "Will I pay extra for a stroller?",
            "Can I check two large suitcases?",
            "What is the fee for overweight checked baggage?",
            "Is a small handbag included in carry-on?",
            "Tell me the bag allowance for this fare.",
            "What happens if my suitcase exceeds 23 kg?"
        ],
        ["OTHER"] = [
            "What museums are worth visiting in Rome?",
            "What's the weather in Copenhagen this weekend?",
            "Can you translate this sentence into Spanish?",
            "What's the capital of Portugal?",
            "Where is the nearest art gallery?",
            "Which currency is used in Sweden?",
            "Recommend a novel for my trip.",
            "What is a traditional dish from Japan?",
            "How many people live in Helsinki?",
            "Tell me about the history of Athens.",
            "How do I say thank you in French?",
            "What time zone is Budapest in?",
            "What language is spoken in Estonia?",
            "Suggest a film set in Paris.",
            "How do I convert Celsius to Fahrenheit?",
            "What is the tallest mountain in Europe?"
        ]
    };
}
