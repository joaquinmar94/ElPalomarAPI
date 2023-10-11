namespace ElPalomar.Models
{
	public class TicketLineAddin
	{
        public int TicketLineId { get; set; }
        public TicketLine TicketLine { get; set; }
        public int AddinIndex { get; set; }
    }
}
