using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("TOUR_BOOKING")]
[Index("UserId", "TourId", Name = "UQ_TOUR_BOOKING", IsUnique = true)]
public partial class TourBooking
{
    [Key]
    [Column("Booking_ID")]
    public int BookingId { get; set; }

    [Column("User_ID")]
    public int UserId { get; set; }

    [Column("Tour_ID")]
    public int TourId { get; set; }

    [Column("Booking_Date")]
    public DateOnly BookingDate { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [ForeignKey("TourId")]
    [InverseProperty("TourBookings")]
    public virtual Tour Tour { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TourBookings")]
    public virtual Visitor User { get; set; } = null!;
}
