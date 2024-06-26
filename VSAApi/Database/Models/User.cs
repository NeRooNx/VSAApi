﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitchProject1Model.Model;

[Table("USER")]
public partial class User
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [Column("NAME")]
    [StringLength(50)]
    public string? Name { get; set; }

    [Column("LAST_NAME_1")]
    [StringLength(50)]
    public string? LastName1 { get; set; }

    [Column("LAST_NAME_2")]
    [StringLength(50)]
    public string? LastName2 { get; set; }

    [Column("BIRTH_DATE", TypeName = "datetime")]
    public DateTime? BirthDate { get; set; }

    [Column("PASSWORD")]
    [StringLength(50)]
    public string? Password { get; set; }
}