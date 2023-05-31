using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Companies
{
    public class EliminarEmpresas
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Id { get; set; }
    }
}
