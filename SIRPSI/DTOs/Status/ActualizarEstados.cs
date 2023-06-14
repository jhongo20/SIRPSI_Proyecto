using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Status
{
    public class ActualizarEstados
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Descripcion { get; set; }
    }
}
