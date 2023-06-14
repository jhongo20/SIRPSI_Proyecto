using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.RolesUsuario
{
    public class ActualizarRolesUsuario
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? UserId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? RoleId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdEstado { get; set; }
    }
}
