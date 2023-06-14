using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Companies
{
    public class ActualizarEmpresas
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? TipoDocumento { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Documento { get; set; } = null!;
        public string? DigitoVerificacion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdTipoEmpresa { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Nombre { get; set; } 
        public string? Descripcion { get; set; }
        public string? Observacion { get; set; }
    }
}
