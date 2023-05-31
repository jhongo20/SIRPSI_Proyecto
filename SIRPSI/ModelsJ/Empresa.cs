using System;
using System.Collections.Generic;

namespace SIRPSI.ModelsJ;

public partial class Empresa
{
    public string Id { get; set; } = null!;

    public int IdConsecutivo { get; set; }

    public string? TipoDocumento { get; set; }

    public string? DigitoVerificacion { get; set; }

    public string? IdTipoEmpresa { get; set; }

    public string Documento { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? Observacion { get; set; }

    public string? IdEstado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime? FechaModifico { get; set; }

    public string? UsuarioModifico { get; set; }

    public virtual TiposEmpresa? IdTipoEmpresaNavigation { get; set; }
}
