using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("ARTIST")]
public partial class Artist
{
    [Key]
    [Column("Artist_ID")]
    public int ArtistId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Surname { get; set; } = null!;

    [Column("Birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column("Death_date")]
    public DateOnly? DeathDate { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    [InverseProperty("Artist")]
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();

    [InverseProperty("Artist")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [ForeignKey("ArtistId")]
    [InverseProperty("Artists")]
    public virtual ICollection<ArtMovement> Movements { get; set; } = new List<ArtMovement>();
}
