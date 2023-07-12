using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.User
{
    //Clase que recibe los datos del cliente, cuando se envian las credenciales
    public class UserCredentials
    {
        public string? IdTypeDocument { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Document { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Password { get; set; }
        public string? IdCompany { get; set; }
        public string? IdCountry{ get; set; }    
        public string? Names { get; set; }
        public string? Surnames { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IdRol { get; set; }
        public string? IdEstado { get; set; }

    }
}
