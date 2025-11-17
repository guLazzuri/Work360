using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Work360.Domain.Enum;

namespace Work360.Domain.Entity
{

    [Table("Work360_Task")]

    public class Tasks
    {
        [Key]
        public required Guid TaskID { get; set; } = Guid.NewGuid();

        public required Guid UserID { get; set; }

        public required string Title { get; set; }

        public required Priority Priority { get; set; }

        public required int EstimateMinutes { get; set; }

        public required string Description { get; set; }

        public TaskSituation TaskSituation { get; set; } = TaskSituation.OPEN;

        public DateTime CreatedTask { get; set; } = DateTime.UtcNow;

        public DateTime FinalDateTask { get; set; }

        public int SpentMinutes { get; set; }


    }
}