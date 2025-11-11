using FastBoard.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastBoard.Data;

public class FastBoardContext : DbContext
{
    public FastBoardContext(DbContextOptions<FastBoardContext> options) : base(options)
    {
    }

    public DbSet<Playbook> Playbooks => Set<Playbook>();
    public DbSet<Play> Plays => Set<Play>();
    public DbSet<Frame> Frames => Set<Frame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Playbook>().HasMany(p => p.Plays).WithOne().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Play>().HasMany(p => p.Frames).WithOne().OnDelete(DeleteBehavior.Cascade);
    }
}
