using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("ADMIN")]
public partial class Admin
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Admin")]
    public virtual User User { get; set; } = null!;
}
