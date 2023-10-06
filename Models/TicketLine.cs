namespace ElPalomar.Models
{
	public class TicketLine
	{
        public int Id { get; set; }
        public Guid GlobalId { get; set; }
        public int TicketId { get; set; }
		public Ticket Ticket { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
