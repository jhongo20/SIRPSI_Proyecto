using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.Roles
{
    public class RegistrarRol
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Status { get; set; }
        public string? Description { get; set; }
    }
}
