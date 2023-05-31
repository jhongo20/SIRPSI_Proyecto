using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class AspNetUserRole
{
    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? UserRegistration { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? UserModify { get; set; }

    public string Discriminator { get; set; } = null!;

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
