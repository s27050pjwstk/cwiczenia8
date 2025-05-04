namespace WebApplication2.Models
{
    public class ClientTripDTO
    {
        public int IdTrip { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Countries { get; set; }
    }
}
