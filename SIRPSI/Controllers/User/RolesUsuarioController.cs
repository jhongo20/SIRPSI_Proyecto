using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Rols;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.User.Roles;
using SIRPSI.DTOs.User.RolesUsuario;
using SIRPSI.Helpers.Answers;
using SIRPSI.ModelsJ;
using System.Security.Claims;

namespace SIRPSI.Controllers.User
{
    [Route("api/rolesusuario")]
    [ApiController]
    public class RolesUsuarioController : ControllerBase
    {
        #region Dependencias
        private readonly UserManager<IdentityUser> userManager;
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        //Constructor 
        public RolesUsuarioController(AppDbContext context,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILoggerManager logger,
            IMapper mapper,
            IEmailSender emailSender)
        {
            this.context = context;
            this.configuration = configuration;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.mapper = mapper;
            this.emailSender = emailSender;
        }
        #endregion

        #region Consulta
        [HttpGet("ConsultarRolesUsuario", Name = "consultarRolesUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get([FromBody] ConsultarRolesUsuario consultarRolesUsuario)
        {
            try
            {

                //Consulta el rol
                var roles = context.AspNetUserRoles.Where(x => x.IdEstado.Equals(consultarRolesUsuario.IdEstado)).Select(x => new
                {
                    x.UserId,
                    x.RoleId,
                    x.IdEstado,

                }).ToList();

                if (roles == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar roles usuario",
                        status = 404,
                        message = "Rol no encontrado"
                    });
                }

                //Retorno de los datos encontrados
                return roles;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError(ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Consultar roles usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion

        #region Registro
        [HttpPost("RegistrarRolesUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarRolesUsuario registrarRolesUsuario)
        {
            try
            {
                //Claims de usuario - Enviados por token
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                }

                var documento = identity.FindFirst("documento").Value.ToString();
                //Consulta de usuarios por documento
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar roles usuario",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estados = await context.estados.ToListAsync();
                //Mapeo de datos en clases
                var roles = mapper.Map<UserRoles>(registrarRolesUsuario);

                //Valores asignados
                roles.Id = Guid.NewGuid().ToString();
                roles.UserId = registrarRolesUsuario.UserId != null ? registrarRolesUsuario.UserId : "";
                roles.RoleId = registrarRolesUsuario.RoleId != null ? registrarRolesUsuario.RoleId : "";
                roles.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(1)).Select(x => x.Id).First();
                roles.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                roles.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                roles.UsuarioModifico = null;
                roles.UsuarioModifico = null;
                roles.Discriminator = "UserRoles";

                //Agregar datos al contexto
                context.Add(roles);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar roles usuario",
                    status = 201,
                    message = "Rol creado"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError(ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar roles usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarRolesUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarRolesUsuario editarRolesUsuario)
        {
            try
            {
                //Claims de usuario - Enviados por token
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                }

                var documento = identity.FindFirst("documento").Value.ToString();
                //Consulta de usuario
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Editar roles usuario",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de roles de usuario
                var existe = await context.AspNetRoles.Where(x => x.Id.Equals(editarRolesUsuario.RoleId)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar roles usuario",
                        status = 404,
                        message = "Rol usuario no encontrado"
                    });
                }
                //Consulta de estados
                var estado = await context.estados.Where(x => x.Id.Equals(editarRolesUsuario.IdEstado)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar roles usuario",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                //Registro de datos
                context.AspNetUserRoles.Where(x => x.UserId.Equals(existe.Id)).ToList()
                    .ForEach(r =>
                    {
                        r.UserId = editarRolesUsuario.UserId != null ? editarRolesUsuario.UserId : "";
                        r.RoleId = existe.Id != null ? existe.Id : "";
                        r.IdEstado = estado.Id;
                        r.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                        r.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                    });

                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar roles usuario",
                    status = 200,
                    message = "Rol usuario actualizado"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar roles usuario " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar roles usuario",
                    status = 400,
                    message = ""
                });
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarRolesUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarRolesUsuario eliminarRolesUsuario)
        {
            try
            {
                //Claims de usuario - Enviados por token
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                }

                var documento = identity.FindFirst("documento").Value.ToString();
                //Consulta de usuario
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar roles usuario",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Consulta de roles usuario
                var existeRol = await context.AspNetUserRoles.Where(x => x.Id.Equals(eliminarRolesUsuario.Id)).FirstOrDefaultAsync();

                if (existeRol == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar roles usuario",
                        status = 404,
                        message = "Datos no encontrados"
                    });
                }
                //Agregar datos al contexto

                //Consulta de estados
                var estados = await context.estados.ToListAsync();

                if (estados == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar roles usuario",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                context.AspNetUserRoles.Where(x => x.Id.Equals(x.Id.Equals(existeRol.Id))).ToList()
                   .ForEach(r =>
                   {
                       r.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
                       r.UsuarioModifico = usuario.Document;
                       r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime; ;
                   });

                //Se elimina el regitro - Logico
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Eliminar usuarios",
                    status = 200,
                    message = "Rol eliminado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError(ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Eliminar usuarios",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
