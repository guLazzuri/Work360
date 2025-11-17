using System.Text.Json.Serialization;

namespace Work360.Domain.Enum
{


    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum Priority
    {
        LOW,
        MEDIUM,
        HIGH
    }
}