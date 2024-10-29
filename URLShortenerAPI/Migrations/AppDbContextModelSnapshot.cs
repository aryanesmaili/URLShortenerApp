﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using URLShortenerAPI.Data;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("URLCategoryModelURLModel", b =>
                {
                    b.Property<int>("CategoriesID")
                        .HasColumnType("integer");

                    b.Property<int>("URLsID")
                        .HasColumnType("integer");

                    b.HasKey("CategoriesID", "URLsID");

                    b.HasIndex("URLsID");

                    b.ToTable("URLCategoryModelURLModel");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.Analytics.URLAnalyticsModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<int>("ClickCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastTimeCalculated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("MostUsedDevicesJSON")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MostUsedLocationsJSON")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("URLID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("URLID")
                        .IsUnique();

                    b.ToTable("URLAnalytics");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.ClickInfoModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<DateTime>("ClickedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DeviceInfoID")
                        .HasColumnType("integer");

                    b.Property<string>("IPAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("LocationID")
                        .HasColumnType("integer");

                    b.Property<int>("URLID")
                        .HasColumnType("integer");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("URLID");

                    b.ToTable("Clicks");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.DeviceInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("BrowserFamily")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ClickID")
                        .HasColumnType("integer");

                    b.Property<string>("OSFamily")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("ClickID")
                        .IsUnique();

                    b.ToTable("DeviceInfos");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.LocationInfo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ClickID")
                        .HasColumnType("integer");

                    b.Property<string>("Continent")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Latitude")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Longitude")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("ClickID")
                        .IsUnique();

                    b.ToTable("LocationInfos");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.URL.URLModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<int?>("CategoryID")
                        .HasColumnType("integer");

                    b.Property<int>("ClickCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("LongURL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ShortCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("URLAnalyticsID")
                        .HasColumnType("integer");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("URLs");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.URLCategory.URLCategoryModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("URLCategories");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.User.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Revoked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.User.UserModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ResetCode")
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("URLCategoryModelURLModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.URLCategory.URLCategoryModel", null)
                        .WithMany()
                        .HasForeignKey("CategoriesID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("URLShortenerAPI.Data.Entities.URL.URLModel", null)
                        .WithMany()
                        .HasForeignKey("URLsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.Analytics.URLAnalyticsModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.URL.URLModel", "URL")
                        .WithOne("URLAnalytics")
                        .HasForeignKey("URLShortenerAPI.Data.Entities.Analytics.URLAnalyticsModel", "URLID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("URL");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.ClickInfoModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.URL.URLModel", "URL")
                        .WithMany("Clicks")
                        .HasForeignKey("URLID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("URL");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.DeviceInfo", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.ClickInfo.ClickInfoModel", "ClickInfo")
                        .WithOne("DeviceInfo")
                        .HasForeignKey("URLShortenerAPI.Data.Entities.ClickInfo.DeviceInfo", "ClickID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("URLShortenerAPI.Data.Entities.ClickInfo.ClientInfo", "Client", b1 =>
                        {
                            b1.Property<int>("DeviceInfoID")
                                .HasColumnType("integer");

                            b1.Property<string>("Engine")
                                .HasColumnType("text");

                            b1.Property<string>("EngineVersion")
                                .HasColumnType("text");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<string>("Type")
                                .HasColumnType("text");

                            b1.Property<string>("Version")
                                .HasColumnType("text");

                            b1.HasKey("DeviceInfoID");

                            b1.ToTable("DeviceInfos");

                            b1.WithOwner()
                                .HasForeignKey("DeviceInfoID");
                        });

                    b.OwnsOne("URLShortenerAPI.Data.Entities.ClickInfo.Device", "Device", b1 =>
                        {
                            b1.Property<int>("DeviceInfoID")
                                .HasColumnType("integer");

                            b1.Property<string>("Brand")
                                .HasColumnType("text");

                            b1.Property<string>("Model")
                                .HasColumnType("text");

                            b1.Property<string>("Type")
                                .HasColumnType("text");

                            b1.HasKey("DeviceInfoID");

                            b1.ToTable("DeviceInfos");

                            b1.WithOwner()
                                .HasForeignKey("DeviceInfoID");
                        });

                    b.OwnsOne("URLShortenerAPI.Data.Entities.ClickInfo.OSInfo", "OS", b1 =>
                        {
                            b1.Property<int>("DeviceInfoID")
                                .HasColumnType("integer");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<string>("Platform")
                                .HasColumnType("text");

                            b1.Property<string>("Version")
                                .HasColumnType("text");

                            b1.HasKey("DeviceInfoID");

                            b1.ToTable("DeviceInfos");

                            b1.WithOwner()
                                .HasForeignKey("DeviceInfoID");
                        });

                    b.Navigation("ClickInfo");

                    b.Navigation("Client")
                        .IsRequired();

                    b.Navigation("Device")
                        .IsRequired();

                    b.Navigation("OS")
                        .IsRequired();
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.LocationInfo", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.ClickInfo.ClickInfoModel", "ClickInfo")
                        .WithOne("PossibleLocation")
                        .HasForeignKey("URLShortenerAPI.Data.Entities.ClickInfo.LocationInfo", "ClickID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClickInfo");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.URL.URLModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.User.UserModel", "User")
                        .WithMany("URLs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.URLCategory.URLCategoryModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.User.UserModel", "User")
                        .WithMany("URLCategories")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.User.RefreshToken", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entities.User.UserModel", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.ClickInfo.ClickInfoModel", b =>
                {
                    b.Navigation("DeviceInfo")
                        .IsRequired();

                    b.Navigation("PossibleLocation")
                        .IsRequired();
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.URL.URLModel", b =>
                {
                    b.Navigation("Clicks");

                    b.Navigation("URLAnalytics");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entities.User.UserModel", b =>
                {
                    b.Navigation("RefreshTokens");

                    b.Navigation("URLCategories");

                    b.Navigation("URLs");
                });
#pragma warning restore 612, 618
        }
    }
}
