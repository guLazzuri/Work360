using System.Text.Json.Serialization;

namespace Work360.Domain.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum EventType
    {
        START_FOCUS_SESSION,
        END_FOCUS_SESSION,
    }
}
