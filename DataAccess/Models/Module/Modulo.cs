using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Module
{
    [Table("Modulo", Schema = "sirpsi")]
    public partial class Modulo
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string? IdEstado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; } = null!;
        public DateTime? FechaModifico { get; set; }
        public string? UsuarioModifico { get; set; }
    }
}
