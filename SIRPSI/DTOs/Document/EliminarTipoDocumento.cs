using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Document
{
    public class EliminarTipoDocumento
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
    }
}
