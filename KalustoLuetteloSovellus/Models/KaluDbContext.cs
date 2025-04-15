using System;
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

    public virtual DbSet<Kategoria> Kategoriat { get; set; }

    public virtual DbSet<Käyttäjä> Käyttäjät { get; set; }

    public virtual DbSet<Rooli> Roolit { get; set; }

    public virtual DbSet<Status> Statukset { get; set; }

    public virtual DbSet<Tapahtuma> Tapahtumat { get; set; }

    public virtual DbSet<Toimipiste> Toimipisteet { get; set; }

    public virtual DbSet<Tuote> Tuotteet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategoria>(entity =>
         {
            entity.ToTable("Kategoria");
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

            entity.HasOne(d => d.Status).WithMany(p => p.Tapahtumas)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tapahtuma_Status");

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

        modelBuilder.Entity<Tuote>((Action<Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Tuote>>)(entity =>
        {
            entity.ToTable("Tuote");

            entity.Property(e => e.Aktiivinen).HasDefaultValue(true);
            entity.Property(e => e.IdNumero)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property((System.Linq.Expressions.Expression<Func<Tuote, byte[]?>>)(e => e.Kuva)).HasColumnType("image");
            entity.Property((System.Linq.Expressions.Expression<Func<Tuote, string?>>)(e => e.Kuvaus))
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Kategoria).WithMany((System.Linq.Expressions.Expression<Func<Kategoria, IEnumerable<Tuote>?>>?)(p => p.Tuotes))
                .HasForeignKey(d => d.KategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tuote_Kategoria");

            entity.HasOne(d => d.Toimipiste).WithMany(p => p.Tuotes)
                .HasForeignKey(d => d.ToimipisteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tuote_Toimipiste");
        }));

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
