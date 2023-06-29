using System.ComponentModel.DataAnnotations.Schema;

namespace SIRPSI.DTOs.Companies
{
    public class ConsultarEmpresas
    {
        public string? Id { get; set; }
        public string? TipoDocumento { get; set; }
        public string? DigitoVerificacion { get; set; }
        public string? IdTipoEmpresa { get; set; }
        public string? Documento { get; set; }
        public string? Nombre { get; set; }
        public string IdMinisterio { get; set; }
        public string? IdEstado { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaModifico { get; set; }

    }
}
