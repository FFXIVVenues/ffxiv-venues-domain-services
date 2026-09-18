using FFXIVVenues.DomainData.Entities;
using FFXIVVenues.DomainData.Entities.Favorites;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace FFXIVVenues.DomainData.Context;

public class DomainDataContext(DbContextOptions<DomainDataContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Flag> Flags { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Opening> Openings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Venues");
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Venues"));
        optionsBuilder.UseLazyLoadingProxies();
    }
}
