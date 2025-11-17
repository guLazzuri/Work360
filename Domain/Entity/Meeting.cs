using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Work360.Domain.Entity
{

    [Table("Work360_Meeting")]

    public class Meeting
    {
        [Key]
        public required Guid MeetingID { get; set; } = Guid.NewGuid();

        public required Guid UserID { get; set; }

        public required string Title { get; set; }

        public required DateTime StartDate { get; set; } = DateTime.UtcNow;

        public required string Description { get; set; }

        public DateTime? EndDate { get; set; }
        public int? minutesDuration { get; set; }

    }
}