using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Work360.Domain.Enum;
using Work360.Infrastructure.Attributes;

namespace Work360.Domain.Entity
{

    [Table("Work360_Task")]

    public class Tasks
    {
        [Key]
        public Guid TaskID { get; set; } = Guid.NewGuid();
        [NotEmptyGuid(ErrorMessage = "UserID cannot be empty GUID.")]
        public Guid UserID { get; set; }
        [Required(ErrorMessage ="Title cannot be empty")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Priority cannot be empty")]
        public  Priority Priority { get; set; }
        [Required(ErrorMessage = "Estimate minutes cannot be empty")]
        public int EstimateMinutes { get; set; }
        [Required(ErrorMessage = "Description minutes cannot be empty")]
        public required string Description { get; set; }

        public TaskSituation TaskSituation { get; set; } = TaskSituation.OPEN;

        public DateTime CreatedTask { get; set; } = DateTime.UtcNow;

        public DateTime? FinalDateTask { get; set; }

        public int SpentMinutes { get; set; }


    }
}