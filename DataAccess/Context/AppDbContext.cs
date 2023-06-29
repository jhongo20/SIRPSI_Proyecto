
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection.Emit;
using DataAccess.Models.Companies;
using DataAccess.Models.Estados;
using DataAccess.Models.Status;
using DataAccess.Models.Country;
using DataAccess.Models.Documents;
using DataAccess.Models.Users;
using DataAccess.Models.Rols;
using DataAccess.Models.Permissions;
using DataAccess.Models.Ministry;
using DataAccess.Models.Module;
using DataAccess.Models.WorkPlace;

namespace DataAccess.Context
{
    public partial class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        }

        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<Roles> AspNetRoles { get; set; }
        public DbSet<UserRoles> AspNetUserRoles { get; set; }
        public DbSet<Empresas> empresas { get; set; }
        public DbSet<Estados> estados { get; set; }
        public DbSet<TiposEmpresa> tiposEmpresas { get; set; }
        public DbSet<Pais> pais { get; set; }
        public DbSet<TiposDocumento> tiposDocumento { get; set; }
        public DbSet<PermisosXUsuario> permisosXUsuario { get; set; }
        public DbSet<Ministerio> ministerio { get; set; }
        public DbSet<Modulo> modulo { get; set; }
        public DbSet<CentroTrabajo> centroTrabajo { get; set; }


        //private void SeeData(ModelBuilder modelBuilder)
        //{
        //    var rolAdminId = "3134ea79-fd86-4f89-9784-cbf6b6b18f82";
        //    var userAdminId = "f1173d7f-eddd-4194-bd76-2c6244b2224c";
        //    var userName = "john.culma@outlook.com";

        //    var rolAdmin = new IdentityRole()
        //    {
        //        Id = rolAdminId,
        //        Name = "Administrador",
        //        NormalizedName = "Administrador"
        //    };
        //    var passwordHasher = new PasswordHasher<IdentityUser>();
        //    var usuarioAdmin = new IdentityUser()
        //    {
        //        Id = userAdminId,
        //        UserName = userName,
        //        NormalizedUserName = userName,
        //        Email = userName,
        //        NormalizedEmail = userName,
        //        PasswordHash = passwordHasher.HashPassword(null, "John.fcl14*")
        //    };
        //    modelBuilder.Entity<IdentityUser>()
        //        .HasData(usuarioAdmin);
        //    modelBuilder.Entity<IdentityRole>()
        //        .HasData(rolAdmin);
        //    modelBuilder.Entity<IdentityUserClaim<string>>()
        //        .HasData(new IdentityUserClaim<string>()
        //        {
        //            Id = 1,
        //            ClaimType = ClaimTypes.Role,
        //            UserId = userAdminId,
        //            ClaimValue = "Administrador"
        //        });
        //}
    }
}
