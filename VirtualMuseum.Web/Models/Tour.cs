using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("TOUR")]
public partial class Tour
{
    [Key]
    [Column("Tour_ID")]
    public int TourId { get; set; }

    [Column("Exhibition_ID")]
    public int ExhibitionId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Column("Tour_Date")]
    public DateOnly TourDate { get; set; }

    [Column("Start_Time")]
    [Precision(0)]
    public TimeOnly StartTime { get; set; }

    [Column("End_Time")]
    [Precision(0)]
    public TimeOnly EndTime { get; set; }

    public int Capacity { get; set; }

    [StringLength(50)]
    public string Language { get; set; } = null!;

    [Column("Guide_User_ID")]
    public int GuideUserId { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [ForeignKey("ExhibitionId")]
    [InverseProperty("Tours")]
    public virtual Exhibition Exhibition { get; set; } = null!;

    [ForeignKey("GuideUserId")]
    [InverseProperty("Tours")]
    public virtual Staff GuideUser { get; set; } = null!;

    [InverseProperty("Tour")]
    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
}
