using Microsoft.EntityFrameworkCore;

public class ReadModelDbContext : DbContext
{
    public ReadModelDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected ReadModelDbContext()
    {
    }

    public DbSet<PartReadModel> Parts { get; set; }

    public DbSet<PartDataReferenceReadModel> PartDataReferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var part = modelBuilder.Entity<PartReadModel>();
        part.HasKey(x => x.Id);
        part.HasMany(x => x.DataReferences)
            .WithOne()
            .HasForeignKey(x => x.ParentId)
            .IsRequired()
            .HasPrincipalKey(x => x.Id);

        part.HasMany(x => x.ExternalIds)
            .WithOne()
            .HasForeignKey(x => x.PartId)
            .IsRequired()
            .HasPrincipalKey(x => x.Id);

        modelBuilder.Entity<PartDataReferenceReadModel>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<PartExternalIdReadModel>()
            .HasKey(x => x.Id);
    }
}
