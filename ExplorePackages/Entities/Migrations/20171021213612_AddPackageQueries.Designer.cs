﻿// <auto-generated />
using Knapcode.ExplorePackages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Knapcode.ExplorePackages.Entities.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("20171021213612_AddPackageQueries")]
    partial class AddPackageQueries
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("Knapcode.ExplorePackages.Entities.Cursor", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<DateTime>("Value");

                    b.HasKey("Name");

                    b.ToTable("Cursors");
                });

            modelBuilder.Entity("Knapcode.ExplorePackages.Entities.Package", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("PackageKey");

                    b.Property<bool>("Deleted");

                    b.Property<long?>("FirstCommitTimestamp");

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<string>("Identity")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<long?>("LastCommitTimestamp");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.HasKey("Key");

                    b.HasIndex("Identity")
                        .IsUnique();

                    b.HasIndex("Id", "Version")
                        .IsUnique();

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("Knapcode.ExplorePackages.Entities.PackageQuery", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("PackageQueryKey");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.HasKey("Key");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("PackageQueries");
                });

            modelBuilder.Entity("Knapcode.ExplorePackages.Entities.PackageQueryMatch", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("PackageQueryMatchKey");

                    b.Property<int>("PackageKey");

                    b.Property<int>("PackageQueryKey");

                    b.HasKey("Key");

                    b.HasIndex("PackageKey");

                    b.HasIndex("PackageQueryKey");

                    b.ToTable("PackageQueryMatches");
                });

            modelBuilder.Entity("Knapcode.ExplorePackages.Entities.PackageQueryMatch", b =>
                {
                    b.HasOne("Knapcode.ExplorePackages.Entities.Package", "Package")
                        .WithMany()
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Knapcode.ExplorePackages.Entities.PackageQuery", "PackageQuery")
                        .WithMany()
                        .HasForeignKey("PackageQueryKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
