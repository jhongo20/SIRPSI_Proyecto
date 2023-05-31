using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User
{
    public class RecoverPassword
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Document { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string ConfirmPassword { get; set; }
   
    }

}
