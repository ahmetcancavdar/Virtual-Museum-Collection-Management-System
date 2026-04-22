using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("TAG")]
public partial class Tag
{
    [Key]
    [Column("Tag_ID")]
    public int TagId { get; set; }

    [StringLength(200)]
    public string? Technique { get; set; }

    [StringLength(200)]
    public string? Medium { get; set; }

    [StringLength(100)]
    public string? Era { get; set; }

    [Column("Artist_ID")]
    public int? ArtistId { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("Tags")]
    public virtual Artist? Artist { get; set; }

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
