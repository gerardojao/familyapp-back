using System;
using System.Collections.Generic;
using FamilyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Data;

public partial class dbContext : DbContext
{
    public dbContext()
    {
    }

    public dbContext(DbContextOptions<dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Egreso> Egresos { get; set; }

    public virtual DbSet<FichaEgreso> FichaEgresos { get; set; }

    public virtual DbSet<FichaIngreso> FichaIngresos { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<AppUser> Users { get; set; }

    public DbSet<PasswordReset> PasswordResets { get; set; } = default!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=familyApp;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=false");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
          
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsUnicode(false);
            entity.Property(u => u.Role).IsUnicode(false);
        });

        modelBuilder.Entity<FichaEgreso>(entity =>
        {
            entity.ToTable("FichaEgreso");

            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Foto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Mes)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsRequired(false);
            entity.Property(e => e.Eliminado).HasDefaultValue(false);
        });

        modelBuilder.Entity<FichaIngreso>(entity =>
        {
            entity.ToTable("FichaIngreso");

            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Foto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Mes)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsRequired(false);
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.ToTable("Ingreso");

            entity.Property(e => e.NombreIngreso)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
