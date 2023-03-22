using System;
using System.Collections.Generic;
using FamilyApp.Entities;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("server=DESKTOP-O0NC63R\\SQLEXPRESS;database=familyApp;trusted_connection=true;encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(10)
                .IsUnicode(false);
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
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.ToTable("Ingreso");

            entity.Property(e => e.NombreIngreso)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
