using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string? TypeDocument { get; set; }

    public string? Document { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public string? IdCountry { get; set; }

    public string? IdCompany { get; set; }

    public string? IdRol { get; set; }

    public string? Names { get; set; }

    public string? Surnames { get; set; }

    public string? Status { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? UserRegistration { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? UserModify { get; set; }

    public string Discriminator { get; set; } = null!;

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserRole1> AspNetUserRole1s { get; set; } = new List<AspNetUserRole1>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; } = new List<AspNetUserRole>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();
}
