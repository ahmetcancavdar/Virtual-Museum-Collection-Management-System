using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("ART_MOVEMENT")]
public partial class ArtMovement
{
    [Key]
    [Column("Movement_ID")]
    public int MovementId { get; set; }

    [Column("Movement_name")]
    [StringLength(200)]
    public string MovementName { get; set; } = null!;

    [Column("Region_of_origin")]
    [StringLength(200)]
    public string? RegionOfOrigin { get; set; }

    [StringLength(100)]
    public string? Era { get; set; }

    public string? Description { get; set; }

    [ForeignKey("MovementId")]
    [InverseProperty("Movements")]
    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    [ForeignKey("MovementId")]
    [InverseProperty("Movements")]
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
