using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListLift.Infrastructure.Marketplaces.Ebay;

internal sealed class EbayApplicationSettingsConfiguration : IEntityTypeConfiguration<EbayApplicationSettings>
{
    public void Configure(EntityTypeBuilder<EbayApplicationSettings> builder)
    {
        builder.ToTable("EbayApplicationSettings");
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.ProtectedClientId).HasMaxLength(4000).IsRequired();
        builder.Property(settings => settings.ProtectedClientSecret).HasMaxLength(4000).IsRequired();
        builder.Property(settings => settings.UpdatedAtUtc).IsRequired();
    }
}
