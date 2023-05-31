using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class TiposEmpresa
{
    public string Id { get; set; } = null!;

    public int IdConsecutivo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? IdEstado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime? FechaModifico { get; set; }

    public string? UsuarioModifico { get; set; }

    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}
