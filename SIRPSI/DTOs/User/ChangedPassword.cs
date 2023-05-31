using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User
{
    public class ChangedPassword
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Document { get; set; }


    }
}
