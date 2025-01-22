using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<ItemEntity>
    {
        public void Configure(EntityTypeBuilder<ItemEntity> builder)
        {
            builder.HasKey(wli => wli.Id);
            builder.HasOne(wli => wli.List)
                .WithMany(wl => wl.Items)
                .HasForeignKey(wli => wli.ListId);

            builder.HasOne(builder => builder.Owner)
                .WithMany()
                .HasForeignKey(builder => builder.OwnerId);
        }
    }
}
