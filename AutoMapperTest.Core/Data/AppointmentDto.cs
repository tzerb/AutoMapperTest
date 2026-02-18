namespace AutoMapperTest.Core.Data;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ReminderDate { get; set; }
}
