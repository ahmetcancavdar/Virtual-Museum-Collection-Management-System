using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Models;

namespace VirtualMuseum.Web.Data;

public partial class VirtualMuseumDbContext : DbContext
{
    public VirtualMuseumDbContext(DbContextOptions<VirtualMuseumDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<ArtMovement> ArtMovements { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Artwork> Artworks { get; set; }

    public virtual DbSet<ArtworkImageUrl> ArtworkImageUrls { get; set; }

    public virtual DbSet<Exhibition> Exhibitions { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourBooking> TourBookings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VirtualTourPlan> VirtualTourPlans { get; set; } = null!;
    public virtual DbSet<VirtualTourStop> VirtualTourStops { get; set; } = null!;

    public virtual DbSet<Visit> Visits { get; set; }

    public virtual DbSet<Visitor> Visitors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ADMIN_USER");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasMany(d => d.Movements).WithMany(p => p.Artists)
                .UsingEntity<Dictionary<string, object>>(
                    "InfluencedBy",
                    r => r.HasOne<ArtMovement>().WithMany()
                        .HasForeignKey("MovementId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_INFLUENCED_BY_MOVEMENT"),
                    l => l.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_INFLUENCED_BY_ARTIST"),
                    j =>
                    {
                        j.HasKey("ArtistId", "MovementId");
                        j.ToTable("INFLUENCED_BY");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("Artist_ID");
                        j.IndexerProperty<int>("MovementId").HasColumnName("Movement_ID");
                    });
        });

        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.HasOne(d => d.Artist).WithMany(p => p.Artworks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARTWORK_ARTIST");

            entity.HasMany(d => d.Movements).WithMany(p => p.Artworks)
                .UsingEntity<Dictionary<string, object>>(
                    "BelongsTo",
                    r => r.HasOne<ArtMovement>().WithMany()
                        .HasForeignKey("MovementId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BELONGS_TO_MOVEMENT"),
                    l => l.HasOne<Artwork>().WithMany()
                        .HasForeignKey("ArtworkId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BELONGS_TO_ARTWORK"),
                    j =>
                    {
                        j.HasKey("ArtworkId", "MovementId");
                        j.ToTable("BELONGS_TO");
                        j.IndexerProperty<int>("ArtworkId").HasColumnName("Artwork_ID");
                        j.IndexerProperty<int>("MovementId").HasColumnName("Movement_ID");
                    });
        });

        modelBuilder.Entity<ArtworkImageUrl>(entity =>
        {
            entity.HasOne(d => d.Artwork).WithMany(p => p.ArtworkImageUrls)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARTWORK_IMAGE_URL_ARTWORK");
        });

        modelBuilder.Entity<Exhibition>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(d => d.Artworks).WithMany(p => p.Exhibitions)
                .UsingEntity<Dictionary<string, object>>(
                    "Feature",
                    r => r.HasOne<Artwork>().WithMany()
                        .HasForeignKey("ArtworkId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FEATURES_ARTWORK"),
                    l => l.HasOne<Exhibition>().WithMany()
                        .HasForeignKey("ExhibitionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FEATURES_EXHIBITION"),
                    j =>
                    {
                        j.HasKey("ExhibitionId", "ArtworkId");
                        j.ToTable("FEATURES");
                        j.IndexerProperty<int>("ExhibitionId").HasColumnName("Exhibition_ID");
                        j.IndexerProperty<int>("ArtworkId").HasColumnName("Artwork_ID");
                    });
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Staff)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STAFF_USER");

            entity.HasMany(d => d.Exhibitions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "CuratedBy",
                    r => r.HasOne<Exhibition>().WithMany()
                        .HasForeignKey("ExhibitionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CURATED_BY_EXHIBITION"),
                    l => l.HasOne<Staff>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CURATED_BY_STAFF"),
                    j =>
                    {
                        j.HasKey("UserId", "ExhibitionId");
                        j.ToTable("CURATED_BY");
                        j.IndexerProperty<int>("UserId").HasColumnName("User_ID");
                        j.IndexerProperty<int>("ExhibitionId").HasColumnName("Exhibition_ID");
                    });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasOne(d => d.Artist).WithMany(p => p.Tags).HasConstraintName("FK_TAG_ARTIST");

            entity.HasMany(d => d.Artworks).WithMany(p => p.Tags)
                .UsingEntity<Dictionary<string, object>>(
                    "TaggedWith",
                    r => r.HasOne<Artwork>().WithMany()
                        .HasForeignKey("ArtworkId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TAGGED_WITH_ARTWORK"),
                    l => l.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TAGGED_WITH_TAG"),
                    j =>
                    {
                        j.HasKey("TagId", "ArtworkId");
                        j.ToTable("TAGGED_WITH");
                        j.IndexerProperty<int>("TagId").HasColumnName("Tag_ID");
                        j.IndexerProperty<int>("ArtworkId").HasColumnName("Artwork_ID");
                    });
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("Open");

            entity.HasOne(d => d.Exhibition).WithMany(p => p.Tours)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TOUR_EXHIBITION");

            entity.HasOne(d => d.GuideUser).WithMany(p => p.Tours)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TOUR_STAFF");
        });

        modelBuilder.Entity<TourBooking>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("Booked");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourBookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TOUR_BOOKING_TOUR");

            entity.HasOne(d => d.User).WithMany(p => p.TourBookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TOUR_BOOKING_VISITOR");
        });

        modelBuilder.Entity<VirtualTourPlan>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Exhibition).WithOne(p => p.VirtualTourPlan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VIRTUAL_TOUR_PLAN_EXHIBITION");
        });

        modelBuilder.Entity<VirtualTourStop>(entity =>
        {
            entity.HasOne(d => d.Plan).WithMany(p => p.VirtualTourStops)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VIRTUAL_TOUR_STOP_PLAN");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("Planned");

            entity.HasOne(d => d.Exhibition).WithMany(p => p.Visits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VISIT_EXHIBITION");

            entity.HasOne(d => d.User).WithMany(p => p.Visits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VISIT_VISITOR");
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Visitor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VISITOR_USER");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
