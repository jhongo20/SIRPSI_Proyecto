using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User.Roles
{
    public class EliminarRol
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Id { get; set; }
    }
}
