using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class Estado
{
    public string Id { get; set; } = null!;

    public int IdConsecutivo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime? FechaModifico { get; set; }

    public string? UsuarioModifico { get; set; }

    public virtual ICollection<AspNetUserRole1> AspNetUserRole1s { get; set; } = new List<AspNetUserRole1>();
}
