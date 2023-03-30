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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DESKTOP-O0NC63R\\SQLEXPRESS;database=familyApp;trusted_connection=true;encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
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

            //entity
            ////.HasOne(d => d.NombreEgresoNavigation).WithMany(p => p.FichaEgresos)
            //    .HasForeignKey(d => d.NombreEgreso)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_FichaEgreso_Egresos");
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
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
