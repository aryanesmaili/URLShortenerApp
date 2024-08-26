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

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.ClickInfoModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<DateTime>("ClickedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DeviceOS")
                        .HasColumnType("integer");

                    b.Property<string>("IPAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PossibleLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("URLID")
                        .HasColumnType("integer");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("URLID");

                    b.ToTable("Clicks");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLCategory", b =>
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

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLModel", b =>
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

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("UserID");

                    b.ToTable("URLs");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.UserModel", b =>
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

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.ClickInfoModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entites.URLModel", "URL")
                        .WithMany("Clicks")
                        .HasForeignKey("URLID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("URL");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLCategory", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entites.UserModel", "User")
                        .WithMany("URLCategories")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLModel", b =>
                {
                    b.HasOne("URLShortenerAPI.Data.Entites.URLCategory", "Category")
                        .WithMany("URLs")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("URLShortenerAPI.Data.Entites.UserModel", "User")
                        .WithMany("URLs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLCategory", b =>
                {
                    b.Navigation("URLs");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.URLModel", b =>
                {
                    b.Navigation("Clicks");
                });

            modelBuilder.Entity("URLShortenerAPI.Data.Entites.UserModel", b =>
                {
                    b.Navigation("URLCategories");

                    b.Navigation("URLs");
                });
#pragma warning restore 612, 618
        }
    }
}
