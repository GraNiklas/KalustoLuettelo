﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KalustoLuetteloSovellus.Models;

public partial class KaluDbContext : DbContext
{
    public KaluDbContext()
    {
    }

    public KaluDbContext(DbContextOptions<KaluDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Kategorium> Kategoria { get; set; }

    public virtual DbSet<Käyttäjä> Käyttäjäs { get; set; }

    public virtual DbSet<Rooli> Roolis { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Tapahtuma> Tapahtumas { get; set; }

    public virtual DbSet<Toimipiste> Toimipistes { get; set; }

    public virtual DbSet<Tuote> Tuotes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=kaluserver.database.windows.net;Database=KaluDb;User ID=KaluAdmin;Password=SalainenSalasana123!;Encrypt=True;TrustServerCertificate=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategorium>(entity =>
        {
            entity.HasKey(e => e.KategoriaId);

            entity.Property(e => e.KategoriaNimi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Käyttäjä>(entity =>
        {
            entity.ToTable("Käyttäjä");

            entity.Property(e => e.Käyttäjätunnus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Salasana)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Rooli).WithMany(p => p.Käyttäjäs)
                .HasForeignKey(d => d.RooliId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Käyttäjä_Rooli");
        });

        modelBuilder.Entity<Rooli>(entity =>
        {
            entity.ToTable("Rooli");

            entity.Property(e => e.RooliNimi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.Property(e => e.StatusNimi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tapahtuma>(entity =>
        {
            entity.HasKey(e => e.TapahtumaId).HasName("PK_Tapahtumat");

            entity.ToTable("Tapahtuma");

            entity.Property(e => e.Kommentti)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Käyttäjä).WithMany(p => p.Tapahtumas)
                .HasForeignKey(d => d.KäyttäjäId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tapahtuma_Käyttäjä");

            entity.HasOne(d => d.Tuote).WithMany(p => p.Tapahtumas)
                .HasForeignKey(d => d.TuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tapahtumat_Tuote");
        });

        modelBuilder.Entity<Toimipiste>(entity =>
        {
            entity.HasKey(e => e.ToimipisteId).HasName("PK_Toimipiste_1");

            entity.ToTable("Toimipiste");

            entity.Property(e => e.Kaupunki)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Oppilaitos)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Careeria");
            entity.Property(e => e.ToimipisteNimi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tuote>(entity =>
        {
            entity.ToTable("Tuote");

            entity.Property(e => e.Aktiivinen).HasDefaultValue(true);
            entity.Property(e => e.IdNumero)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Kuva).HasColumnType("image");
            entity.Property(e => e.Kuvaus)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Kategoria).WithMany(p => p.Tuotes)
                .HasForeignKey(d => d.KategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tuote_Kategoria");

            entity.HasOne(d => d.Status).WithMany(p => p.Tuotes)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tuote_Status");

            entity.HasOne(d => d.Toimipiste).WithMany(p => p.Tuotes)
                .HasForeignKey(d => d.ToimipisteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tuote_Toimipiste");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
