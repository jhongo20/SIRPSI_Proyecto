using System.ComponentModel.DataAnnotations;

namespace SIRPSI.DTOs.Ministry
{
    public class ConsultarMinisterios
    {
        public string?  Id { get; set; }
        public string? Nombre { get; set; }
        public string? Nit { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModifico { get; set; }
    }
}
