using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Rols
{
    //AspNetRoles
    public partial class Roles : IdentityRole
    {
       
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? UserRegistration { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? UserModify { get; set; }
    }
}
