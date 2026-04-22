using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[PrimaryKey("UserId", "ExhibitionId", "VisitDate")]
[Table("VISIT")]
public partial class Visit
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [Key]
    [Column("Exhibition_ID")]
    public int ExhibitionId { get; set; }

    [Key]
    [Column("Visit_date")]
    public DateOnly VisitDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [ForeignKey("ExhibitionId")]
    [InverseProperty("Visits")]
    public virtual Exhibition Exhibition { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Visits")]
    public virtual Visitor User { get; set; } = null!;
}
