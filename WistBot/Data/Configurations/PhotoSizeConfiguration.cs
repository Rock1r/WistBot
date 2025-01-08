using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class PhotoSizeConfiguration : IEntityTypeConfiguration<PhotoSizeEntity>
    {
        public void Configure(EntityTypeBuilder<PhotoSizeEntity> builder)
        {
            builder.HasKey(ps => ps.Id);
            builder.HasOne(ps => ps.WishListItem)
                .WithOne(wli => wli.Photo)
                .HasForeignKey<WishListItemEntity>(ps => ps.PhotoId);
        }
    }
}
