namespace ClubManagementApi.Models.Entities;

public class VenueSlot
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty; // "Monday", "Tuesday", etc.
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public Venue Venue { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}
