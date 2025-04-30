using System;
using System.Collections.Generic;
using EcoFootprintCalculator.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace EcoFootprintCalculator.Lib;

public partial class MySQL : DbContext
{
    public MySQL(DbContextOptions<MySQL> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Footprint> Footprints { get; set; }

    public virtual DbSet<Preset> Presets { get; set; }

    public virtual DbSet<Travel> Travels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Colour).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Footprint>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Date).HasColumnType("datetime");
        });

        modelBuilder.Entity<Preset>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Travel>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Date).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PRIMARY");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.Pwd).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
