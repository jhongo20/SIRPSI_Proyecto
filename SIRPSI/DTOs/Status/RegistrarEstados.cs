using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Status
{
    public class RegistrarEstados
    {

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; } = null!;
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Descripcion { get; set; } = null!;

    }
}
