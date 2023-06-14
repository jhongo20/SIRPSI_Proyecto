using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Permissions
{
    [Table("PermisosXUsuario", Schema = "sirpsi")]
    public partial class PermisosXUsuario
    {
        public string Id { get; set; } = null!;

        public string? Vista { get; set; }

        public string IdUsuario { get; set; } = null!;

        public string IdEmpresa { get; set; } = null!;

        public bool Consulta { get; set; }

        public bool Registrar { get; set; }

        public bool Actualizar { get; set; }

        public bool Eliminar { get; set; }

        public bool Reportes { get; set; }

        public string? IdEstado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string UsuarioRegistro { get; set; } = null!;

        public DateTime? FechaModifico { get; set; }

        public string? UsuarioModifico { get; set; }
    }
}
