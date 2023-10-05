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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Ticket>().ToTable("Ticket")
				.HasKey(e => e.Id);
		}
	}
}
