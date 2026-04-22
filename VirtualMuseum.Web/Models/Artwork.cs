using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("ARTWORK")]
public partial class Artwork
{
    [Key]
    [Column("Artwork_ID")]
    public int ArtworkId { get; set; }

    [StringLength(300)]
    public string Title { get; set; } = null!;

    [Column("Creation_year")]
    public int? CreationYear { get; set; }

    [Column("Artwork_type")]
    [StringLength(100)]
    public string? ArtworkType { get; set; }

    [StringLength(200)]
    public string? Theme { get; set; }

    [StringLength(200)]
    public string? Technique { get; set; }

    [StringLength(200)]
    public string? Medium { get; set; }

    [StringLength(100)]
    public string? Dimensions { get; set; }

    [Column("Acquisition_date")]
    public DateOnly? AcquisitionDate { get; set; }

    [Column("Artist_ID")]
    public int ArtistId { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("Artworks")]
    public virtual Artist Artist { get; set; } = null!;

    [InverseProperty("Artwork")]
    public virtual ICollection<ArtworkImageUrl> ArtworkImageUrls { get; set; } = new List<ArtworkImageUrl>();

    [ForeignKey("ArtworkId")]
    [InverseProperty("Artworks")]
    public virtual ICollection<Exhibition> Exhibitions { get; set; } = new List<Exhibition>();

    [ForeignKey("ArtworkId")]
    [InverseProperty("Artworks")]
    public virtual ICollection<ArtMovement> Movements { get; set; } = new List<ArtMovement>();

    [ForeignKey("ArtworkId")]
    [InverseProperty("Artworks")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
