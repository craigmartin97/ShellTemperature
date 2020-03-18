﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShellTemperature.Data;

namespace ShellTemperature.Data.Migrations
{
    [DbContext(typeof(ShellDb))]
    [Migration("20200317155427_RemoveRequierdSDCard")]
    partial class RemoveRequierdSDCard
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ShellTemperature.Data.DeviceInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DeviceAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("DevicesInfo");
                });

            modelBuilder.Entity("ShellTemperature.Data.Positions", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("ShellTemperature.Data.ReadingComment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ReadingComments");
                });

            modelBuilder.Entity("ShellTemperature.Data.SdCardShellTemp", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float?>("Latitude")
                        .HasColumnType("real");

                    b.Property<float?>("Longitude")
                        .HasColumnType("real");

                    b.Property<DateTime?>("RecordedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<double>("Temperature")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("SdCardShellTemperatures");
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemp", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float?>("Latitude")
                        .HasColumnType("real");

                    b.Property<float?>("Longitude")
                        .HasColumnType("real");

                    b.Property<DateTime>("RecordedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<double>("Temperature")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("ShellTemperatures");
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemperatureComment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CommentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ShellTempId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CommentId");

                    b.HasIndex("ShellTempId");

                    b.ToTable("ShellTemperatureComments");
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemperaturePosition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PositionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ShellTempId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("PositionId");

                    b.HasIndex("ShellTempId");

                    b.ToTable("ShellTemperaturePositions");
                });

            modelBuilder.Entity("ShellTemperature.Data.SdCardShellTemp", b =>
                {
                    b.HasOne("ShellTemperature.Data.DeviceInfo", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemp", b =>
                {
                    b.HasOne("ShellTemperature.Data.DeviceInfo", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemperatureComment", b =>
                {
                    b.HasOne("ShellTemperature.Data.ReadingComment", "Comment")
                        .WithMany()
                        .HasForeignKey("CommentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShellTemperature.Data.ShellTemp", "ShellTemp")
                        .WithMany()
                        .HasForeignKey("ShellTempId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShellTemperature.Data.ShellTemperaturePosition", b =>
                {
                    b.HasOne("ShellTemperature.Data.Positions", "Position")
                        .WithMany()
                        .HasForeignKey("PositionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShellTemperature.Data.ShellTemp", "ShellTemp")
                        .WithMany()
                        .HasForeignKey("ShellTempId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
