using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class AspNetRole
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? UserRegistration { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? UserModify { get; set; }

    public string Discriminator { get; set; } = null!;

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    public virtual ICollection<AspNetUserRole1> AspNetUserRole1s { get; set; } = new List<AspNetUserRole1>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; } = new List<AspNetUserRole>();
}
