namespace AutoMapperTest.Core.Domain;

public class Appointment
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ReminderDate { get; set; }
}
