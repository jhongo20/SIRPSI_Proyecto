using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.RolesUsuario
{
    public class EliminarRolesUsuario
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }
    }
}
