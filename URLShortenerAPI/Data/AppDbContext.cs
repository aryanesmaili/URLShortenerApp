﻿using Microsoft.EntityFrameworkCore;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.Finance;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;
using URLShortenerAPI.Data.Entities.User;
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

            builder.Entity<UserModel>()
                .HasMany(rt => rt.RefreshTokens)
                .WithOne(u => u.User)
                .HasForeignKey(fk => fk.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<URLModel>()
                .HasMany(c => c.Clicks)
                .WithOne(c => c.URL)
                .HasForeignKey(x => x.URLID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<URLModel>()
                .HasMany(x => x.Categories)
                .WithMany(x => x.URLs);

            builder.Entity<URLModel>()
                .HasOne(x => x.URLAnalytics)
                .WithOne(x => x.URL)
                .HasForeignKey<URLAnalyticsModel>(x => x.URLID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClickInfoModel>()
                .HasOne(x => x.PossibleLocation)
                .WithOne(x => x.ClickInfo)
                .HasForeignKey<LocationInfo>(x => x.ClickID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClickInfoModel>()
                .HasOne(x => x.DeviceInfo)
                .WithOne(x => x.ClickInfo)
                .HasForeignKey<DeviceInfo>(x => x.ClickID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DeviceInfo>().OwnsOne(a => a.Client);
            builder.Entity<DeviceInfo>().OwnsOne(a => a.Device);
            builder.Entity<DeviceInfo>().OwnsOne(a => a.OS);

            builder.Entity<UserModel>()
                .HasOne(x => x.FinancialRecord)
                .WithOne(x => x.User)
                .HasForeignKey<FinancialRecord>(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FinancialRecord>()
                .HasMany(x => x.Deposits)
                .WithOne(x => x.Finance)
                .HasForeignKey(x => x.FinanceID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FinancialRecord>()
                .HasMany(x => x.Purchases)
                .WithOne(x => x.Finance)
                .HasForeignKey(x => x.FinanceID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<URLModel>()
                .HasOne(x => x.Purchase)
                .WithOne(x => x.URL)
                .HasForeignKey<PurchaseModel>(x => x.CustomURLID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserModel>()
                .HasIndex(x => x.FinancialID);

            builder.Entity<DepositModel>()
                .HasIndex(x => x.FinanceID);

            builder.Entity<PurchaseModel>()
                .HasIndex(x => x.FinanceID);

        }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<URLModel> URLs { get; set; }
        public virtual DbSet<ClickInfoModel> Clicks { get; set; }
        public DbSet<URLCategoryModel> URLCategories { get; set; }
        public DbSet<URLAnalyticsModel> URLAnalytics { get; set; }
        public DbSet<LocationInfo> LocationInfos { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<FinancialRecord> FinancialRecords { get; set; }
        public DbSet<PurchaseModel> Purchases { get; set; }
        public DbSet<DepositModel> Deposits { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
