using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Work360.Infrastructure.Attributes;

namespace Work360.Domain.Entity
{
    [Table("Work360_Meeting")]
    public class Meeting
    {
        [Key]
        public Guid MeetingID { get; set; } = Guid.NewGuid();

        [NotEmptyGuid(ErrorMessage = "UserID cannot be empty GUID.")]
        public Guid UserID { get; set; }

        [Required(ErrorMessage = "Title cannot be empty.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description cannot be empty.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }
        public int? MinutesDuration { get; set; }
    }
}
