using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

	public DbSet<Game> Games {get; set;} = null!;
	public DbSet<Score> Scores {get; set;} = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
		{
			throw new InvalidOperationException(
				"DbContextOptions must be configured externally");
		}
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

		modelBuilder.Entity<Game>().HasIndex(g => g.Name).IsUnique();
    }
}