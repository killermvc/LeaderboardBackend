using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

	public DbSet<Game> Games {get; set;} = null!;
	public DbSet<Score> Scores {get; set;} = null!;
	public DbSet<Role> Roles {get; set;} = null!;
	public DbSet<UserRole> UserRoles {get; set;} = null!;

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

		modelBuilder.Entity<UserRole>().HasKey(ur => new {ur.UserId, ur.RoleId});

		modelBuilder.Entity<UserRole>()
			.HasOne(ur => ur.User)
			.WithMany(u => u.UserRoles)
			.HasForeignKey(ur => ur.RoleId);

		modelBuilder.Entity<UserRole>()
			.HasOne(ur => ur.Role)
			.WithMany(r => r.UserRoles)
			.HasForeignKey(ur => ur.RoleId);

		modelBuilder.Entity<Score>()
			.Property(b => b.DateAchieved)
			.HasDefaultValueSql("NOW(6)");
    }
}