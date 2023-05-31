using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Rols
{

    [Table("AspNetUserRoles", Schema = "sirpsi")]
    public partial class UserRoles 
    {

        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string RoleId { get; set; } = null!;

        public string? IdEstado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string UsuarioRegistro { get; set; } = null!;

        public DateTime? FechaModifico { get; set; }

        public string? UsuarioModifico { get; set; }

        public string Discriminator { get; set; } = null!;
    }
}
