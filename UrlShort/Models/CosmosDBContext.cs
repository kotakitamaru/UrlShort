using Microsoft.EntityFrameworkCore;

namespace UrlShort.Models;

public class CosmosDBContext : DbContext
{
    public CosmosDBContext()
    {
        
    }

    public CosmosDBContext(DbContextOptions<CosmosDBContext> options) : base(options)
    {
        
    }
    
    public DbSet<UrlInfo>? UrlsInfos { get; set; }
    public DbSet<User>? Users { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(
            "https://jiscd.documents.azure.com:443/",
            "9EGPWaENKdwzVfwcFzDm2lVr01AQLtUjsIngbIT1z8YyEnwmpDrWDI0qmuV7iYJlRVoZGVRKX8EYACDbBWNGsg==",
            databaseName: "UrlShortener");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UrlInfo>()
            .ToContainer("Urls").
            HasPartitionKey(x => x.Id);
        modelBuilder.Entity<User>()
            .ToContainer("Users").
            HasPartitionKey(x => x.Id);
    }
}