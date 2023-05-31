using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Country
{
    public class RegistrarPais
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdEstado { get; set; }
    }
}
