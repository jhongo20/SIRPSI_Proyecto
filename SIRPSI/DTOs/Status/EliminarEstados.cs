using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Status
{
    public class EliminarEstados
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
    }
}
