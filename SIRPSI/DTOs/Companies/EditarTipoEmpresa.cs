using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Companies
{
    public class EditarTipoEmpresa
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; } = null!;
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string? Descripcion { get; set; }

    }
}
