using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User
{
    public class DeleteUser
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }
    }
}
