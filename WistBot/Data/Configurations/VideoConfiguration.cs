using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WistBot.Data.Models;

namespace WistBot.Data.Configurations
{
    public class VideoConfiguration : IEntityTypeConfiguration<VideoEntity>
    {
        public void Configure(EntityTypeBuilder<VideoEntity> builder)
        {
            builder.HasKey(v => v.Id);
            builder.HasOne(v => v.WishListItem)
                .WithOne(wli => wli.Video)
                .HasForeignKey<WishListItemEntity>(v => v.VideoId);
        }
    }
}
