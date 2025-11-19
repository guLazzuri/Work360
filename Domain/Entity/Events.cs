using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Work360.Domain.Enum;
using Work360.Infrastructure.Attributes;

namespace Work360.Domain.Entity
{
    [Table("Work360_Events")]
    public class Events
    {
        [Key]
        public Guid EventID { get; set; } = Guid.NewGuid();

        [NotEmptyGuid(ErrorMessage = "UserID cannot be empty GUID.")]
        public Guid UserID { get; set; }
        [Required(ErrorMessage = "Event type cannot be empty.")]
        public EventType EventType { get; set; } = EventType.START_FOCUS_SESSION;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public int Duration { get; set; }


    }
}   
