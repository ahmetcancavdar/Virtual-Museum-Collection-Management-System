using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("EXHIBITION")]
public partial class Exhibition
{
    [Key]
    [Column("Exhibition_ID")]
    public int ExhibitionId { get; set; }

    [StringLength(300)]
    public string Title { get; set; } = null!;

    [StringLength(200)]
    public string? Theme { get; set; }

    [Column("Start_date")]
    public DateOnly StartDate { get; set; }

    [Column("End_date")]
    public DateOnly EndDate { get; set; }

    [Column("Room_ID")]
    public int RoomId { get; set; }

    [Column("Room_Name")]
    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    [Column("Is_Active")]
    public bool IsActive { get; set; }

    [InverseProperty("Exhibition")]
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();

    [InverseProperty("Exhibition")]
    public virtual VirtualTourPlan? VirtualTourPlan { get; set; }

    [InverseProperty("Exhibition")]
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    [ForeignKey("ExhibitionId")]
    [InverseProperty("Exhibitions")]
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();

    [ForeignKey("ExhibitionId")]
    [InverseProperty("Exhibitions")]
    public virtual ICollection<Staff> Users { get; set; } = new List<Staff>();
}
