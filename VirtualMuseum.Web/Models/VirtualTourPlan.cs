using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("VIRTUAL_TOUR_PLAN")]
[Index("ExhibitionId", Name = "UQ_VIRTUAL_TOUR_PLAN_Exhibition", IsUnique = true)]
public partial class VirtualTourPlan
{
    [Key]
    [Column("Plan_ID")]
    public int PlanId { get; set; }

    [Column("Exhibition_ID")]
    public int ExhibitionId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Column("Estimated_Duration_Minutes")]
    public int EstimatedDurationMinutes { get; set; }

    [Column("Is_Active")]
    public bool IsActive { get; set; }

    [ForeignKey("ExhibitionId")]
    [InverseProperty("VirtualTourPlan")]
    public virtual Exhibition Exhibition { get; set; } = null!;

    [InverseProperty("Plan")]
    public virtual ICollection<VirtualTourStop> VirtualTourStops { get; set; } = new List<VirtualTourStop>();
}
