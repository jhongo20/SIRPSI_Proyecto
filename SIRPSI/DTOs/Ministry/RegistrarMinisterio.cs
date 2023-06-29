using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Ministry
{
    public class RegistrarMinisterio
    {
        [Required(ErrorMessage = "El {0} Requerido")] 
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El {0} es Requerido")]
        public string Nit { get; set; }
        public string? Descripcion { get; set; }
    }
}

