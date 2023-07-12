using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRPSI.DTOs.Companies
{
    public class RegistrarEmpresas
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? TipoDocumento { get; set; }
        public string? DigitoVerificacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdTipoEmpresa { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string? Observacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdEstado { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string IdMinisterio { get; set; }
    }
}
