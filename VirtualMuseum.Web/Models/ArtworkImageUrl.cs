using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[PrimaryKey("ArtworkId", "ImageUrl")]
[Table("ARTWORK_IMAGE_URL")]
public partial class ArtworkImageUrl
{
    [Key]
    [Column("Artwork_ID")]
    public int ArtworkId { get; set; }

    [Key]
    [Column("Image_url")]
    [StringLength(1000)]
    public string ImageUrl { get; set; } = null!;

    [ForeignKey("ArtworkId")]
    [InverseProperty("ArtworkImageUrls")]
    public virtual Artwork Artwork { get; set; } = null!;
}
