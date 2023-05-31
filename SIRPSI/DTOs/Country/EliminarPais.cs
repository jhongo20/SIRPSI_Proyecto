using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Country
{
    public class EliminarPais
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
  
    }
}
