using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.Roles
{
    public class ActualizarRol
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
