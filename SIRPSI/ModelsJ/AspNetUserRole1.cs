using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class AspNetUserRole1
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public string? IdEstado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime? FechaModifico { get; set; }

    public string? UsuarioModifico { get; set; }

    public string Discriminator { get; set; } = null!;

    public virtual Estado? IdEstadoNavigation { get; set; }

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
