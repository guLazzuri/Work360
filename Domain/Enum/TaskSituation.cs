using System.Text.Json.Serialization;

namespace Work360.Domain.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum TaskSituation
    {
        OPEN,
        IN_PROGRESS,
        COMPLETED
    }
}
