using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("STAFF")]
public partial class Staff
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [InverseProperty("GuideUser")]
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();

    [ForeignKey("UserId")]
    [InverseProperty("Staff")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Exhibition> Exhibitions { get; set; } = new List<Exhibition>();
}
