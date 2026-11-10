using ListLift.Domain.Entities;
using ListLift.Infrastructure.Identity;
using ListLift.Infrastructure.Marketplaces.Ebay;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ListLift.Infrastructure.Persistence;

public sealed class ListingDbContext(DbContextOptions<ListingDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ListingDraft> ListingDrafts => Set<ListingDraft>();
    public DbSet<ListingPhoto> ListingPhotos => Set<ListingPhoto>();
    public DbSet<ChannelListing> ChannelListings => Set<ChannelListing>();
    public DbSet<ListingCategorySelection> ListingCategorySelections => Set<ListingCategorySelection>();
    internal DbSet<EbayApplicationSettings> EbayApplicationSettings => Set<EbayApplicationSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingDbContext).Assembly);
    }
}
