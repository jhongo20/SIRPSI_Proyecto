using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
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
using SIRPSI.Helpers.Answers;
using System.Data;
using System.Security.Claims;

namespace SIRPSI.Controllers.User
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : ControllerBase
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
        public RolesController(AppDbContext context,
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
        [HttpGet("ConsultarRoles", Name = "consultarRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get([FromBody] ConsultarRoles consultarRoles)
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
                        title = "Registrar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estado = await context.estados.Where(x => x.Id.Equals(consultarRoles.Status)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar roles",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Consulta el rol
                var roles = context.AspNetRoles.Where(x => x.Status.Equals(estado.Id)).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.Status

                }).ToList();

                if (roles == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar roles",
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
                logger.LogError("Consultar roles " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Consultar roles",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion

        #region Registro
        [HttpPost("RegistrarRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarRol registrarRoles)
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
                        title = "Registrar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estado = await context.estados.Where(x => x.Id.Equals(registrarRoles.Status)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar roles",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Mapeo de datos en clases
                var roles = mapper.Map<Roles>(registrarRoles);
                //Valores asignados
                roles.Id = Guid.NewGuid().ToString();
                roles.Name = registrarRoles.Name;
                roles.Status = estado.Id;
                roles.ConcurrencyStamp = Guid.NewGuid().ToString();
                roles.Description = registrarRoles.Description;
                roles.UserRegistration = usuario.Document;
                roles.RegistrationDate = DateTime.Now.ToDateTimeZone().DateTime;
                roles.UserModify = null;
                roles.ModifiedDate = null;

                //Agregar datos al contexto
                context.Add(roles);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar roles",
                    status = 201,
                    message = "Rol creado"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar roles " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar roles",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarRol editarRol)
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
                        title = "Editar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de roles de usuario
                var existeRol = await context.AspNetRoles.Where(x => x.Id.Equals(editarRol.Id)).FirstOrDefaultAsync();

                if (existeRol == null)
                {
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar roles",
                        status = 404,
                        message = "Rol no encontrado"
                    });
                }
                //Consulta de estados
                var estados = await context.estados.ToListAsync();
                //Registro de datos
                context.AspNetRoles.Where(x => x.Id.Equals(editarRol.Id)).ToList()
                    .ForEach(r =>
                    {
                        r.Name = editarRol.Name;
                        r.Description = editarRol.Description;
                        r.UserModify = usuario.Document;
                        r.ModifiedDate = DateTime.Now.ToDateTimeZone().DateTime; 
                    });
                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar roles",
                    status = 200,
                    message = "Rol actualizado"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar roles " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar roles",
                    status = 400,
                    message = ""
                }); ;
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarRol eliminarRol)
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
                        title = "Eliminar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estados = await context.estados.ToListAsync();

                //Consulta de roles
                var existeRol = context.AspNetRoles.Where(x => x.Id.Equals(eliminarRol.Id)).FirstOrDefault();

                if (existeRol == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar roles",
                        status = 404,
                        message = "Rol no encontrado"
                    });
                }

                //Agregar datos al contexto
                context.AspNetRoles.Where(x => x.Id.Equals(eliminarRol.Id)).ToList()
                  .ForEach(r =>
                  {
                      r.Status = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
                      r.UserModify = usuario.Document;
                      r.ModifiedDate = DateTime.Now.ToDateTimeZone().DateTime;
                  });

                //Se elimina el regitro
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Eliminar roles",
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
                    title = "Eliminar roles",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

            }
        #endregion

    }
}
