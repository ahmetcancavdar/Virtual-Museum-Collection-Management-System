using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VirtualMuseum.Web.Models;

[Table("USER")]
[Index("Email", Name = "UQ_USER_Email", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Surname { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("Password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [InverseProperty("User")]
    public virtual Admin? Admin { get; set; }

    [InverseProperty("User")]
    public virtual Staff? Staff { get; set; }

    [InverseProperty("User")]
    public virtual Visitor? Visitor { get; set; }
}
