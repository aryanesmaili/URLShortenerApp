using Microsoft.EntityFrameworkCore;
using URLShortenerAPI.Data.Entites;
namespace URLShortenerAPI.Data
{
    internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserModel>()
                .HasMany(u => u.URLs)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserModel>()
                .HasMany(x => x.URLCategories)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<URLModel>()
                .HasMany(c => c.Clicks)
                .WithOne(c => c.URL)
                .HasForeignKey(x => x.URLID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<URLModel>()
                .HasOne(x => x.Category)
                .WithMany(x => x.URLs)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

        }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<URLModel> URLs { get; set; }
        public virtual DbSet<ClickInfoModel> Clicks { get; set; }
        public DbSet<URLCategory> URLCategories { get; set; }
    }
}
