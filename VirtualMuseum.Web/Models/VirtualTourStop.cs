using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("VIRTUAL_TOUR_STOP")]
[Index("PlanId", "StepNo", Name = "UQ_VIRTUAL_TOUR_STOP_Plan_Step", IsUnique = true)]
public partial class VirtualTourStop
{
    [Key]
    [Column("Stop_ID")]
    public int StopId { get; set; }

    [Column("Plan_ID")]
    public int PlanId { get; set; }

    [Column("Step_No")]
    public int StepNo { get; set; }

    [Column("Room_ID")]
    public int RoomId { get; set; }

    [Column("Room_Name")]
    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    [Column("Stop_Title")]
    [StringLength(200)]
    public string StopTitle { get; set; } = null!;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("Estimated_Minutes")]
    public int? EstimatedMinutes { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("VirtualTourStops")]
    public virtual VirtualTourPlan Plan { get; set; } = null!;
}
