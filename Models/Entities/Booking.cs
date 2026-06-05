namespace ClubManagementApi.Models.Entities;

public class Booking
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int VenueId { get; set; }
    public int SlotId { get; set; }
    public DateOnly BookingDate { get; set; }
    public string Status { get; set; } = "Confirmed"; // "Confirmed" | "Cancelled"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Member Member { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public VenueSlot Slot { get; set; } = null!;
}
