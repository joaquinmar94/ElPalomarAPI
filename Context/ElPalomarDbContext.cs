using ElPalomar.Models;
using Microsoft.EntityFrameworkCore;

namespace ElPalomar.Context
{
	public class ElPalomarDbContext : DbContext
	{
        protected readonly IConfiguration _configuration;
        public ElPalomarDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
		}

		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<TicketLine> TicketLines { get; set; }
        public DbSet<TicketLineAddin> TicketLineAddins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TicketLineAddin>().ToTable("TicketLineAddin")
				.HasKey(x => new { x.TicketLineId, x.AddinIndex });
			modelBuilder.Entity<TicketLine>().ToTable("TicketLine")
				.HasMany(x => x.TicketLineAddins)
				.WithOne(x => x.TicketLine)
				.HasForeignKey(x => x.TicketLineId)
				.HasPrincipalKey(e => e.Id)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Ticket>().ToTable("Ticket")
				.HasMany(e => e.TicketLines)
				.WithOne(e => e.Ticket)
				.HasForeignKey(e => e.TicketId)
				.HasPrincipalKey(t => t.Id);
		}
	}
}
