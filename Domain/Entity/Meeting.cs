namespace Work360.Domain.Entity;

public class Meeting
{
    public string MeetingID { get; set; }
    
    public List<User> Participants { get; set; }

    public string Title { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    
}