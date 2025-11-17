using Work360.Domain.Enum;

namespace Work360.Domain.Entity;

public class Task
{
    public Guid TaskID {get; set;}

    public User User {get; set;}

    public Guid UserID {get; set;}

    public string Title {get; set;}

    public Priority Priority {get; set;}

    public int EstimateMinutes {get; set;}

    public string Description {get; set;}

    public bool IsCompleted {get; set;}

    public DateTime CreatedTask {get; set;}

}