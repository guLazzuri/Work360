using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Work360.Domain.Enum;

namespace Work360.Domain.Entity
{
    [Table("Work360_Events")]
    public class Events
    {
        [Key]
        public required Guid EventID { get; set; } = Guid.NewGuid();
        public required Guid UserID { get; set; }
        public EventType EventType { get; set; } = EventType.START_FOCUS_SESSION;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public int? Duration { get; set; }
    }
}
