using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Status
{
    [Table("Empresas", Schema = "sirpsi")]
    public partial class Empresas
    {
        public string Id { get; set; } = null!;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConsecutivo { get; set; }

        public string? TipoDocumento { get; set; }

        public string? DigitoVerificacion { get; set; }

        public string? IdTipoEmpresa { get; set; }

        public string Documento { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string Descripcion { get; set; } = null!;

        public string? Observacion { get; set; }

        public string? IdEstado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string UsuarioRegistro { get; set; } = null!;

        public DateTime? FechaModifico { get; set; }

        public string? UsuarioModifico { get; set; }

    }
}
