using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("VISITOR")]
public partial class Visitor
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();

    [ForeignKey("UserId")]
    [InverseProperty("Visitor")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
