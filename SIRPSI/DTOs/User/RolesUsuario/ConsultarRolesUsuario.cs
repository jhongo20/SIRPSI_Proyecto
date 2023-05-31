using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.RolesUsuario
{
    public class ConsultarRolesUsuario
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? IdEstado { get; set; }
    }
}
