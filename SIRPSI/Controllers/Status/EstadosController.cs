using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.Status;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.Status
{
    [Route("api/estados")]
    [ApiController]
    public class EstadosController : ControllerBase
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
        public EstadosController(AppDbContext context,
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
        [HttpGet("ConsultarEstados", Name = "consultarEstados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get()
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

                //Consulta el estados
                var estados = await context.estados.Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Descripcion
                }).ToListAsync();

                if (estados == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar estados",
                        status = 404,
                        message = "Estados no encontrados"
                    });
                }

                //Retorno de los datos encontrados
                return estados;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar estados " + ex.Message.ToString() + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Consultar estados",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Registro
        [HttpPost("RegistrarEstados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarEstados registrarEstados)
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
                //Consulta de estados 
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar estados",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Mapeo de datos en clases
                var estados = mapper.Map<Estados>(registrarEstados);
                //Valores asignados
                estados.Id = Guid.NewGuid().ToString();
                estados.Nombre = registrarEstados.Nombre != null ? registrarEstados.Nombre : "";
                estados.Descripcion = registrarEstados.Descripcion;
                estados.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                estados.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                estados.UsuarioModifico = null;
                estados.UsuarioModifico = null;

                //Agregar datos al contexto
                context.Add(estados);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar estado",
                    status = 201,
                    message = "Estado creado"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar estado " + ex.Message.ToString() + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar estado",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarEstados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarEstados editarEstados)
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
                        title = "Editar estado",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de estados de usuario
                var existeEstado = await context.estados.Where(x => x.Id.Equals(editarEstados.Id)).FirstOrDefaultAsync();

                if (existeEstado == null)
                {
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar estado",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                //Registro de datos
                context.estados.Where(x => x.Id.Equals(existeEstado.Id)).ToList()
                    .ForEach(e =>
                    {
                        e.Nombre = editarEstados.Nombre != null ? editarEstados.Nombre : "";
                        e.Descripcion = editarEstados.Descripcion;
                        e.UsuarioModifico = usuario.Document;
                        e.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                    });

                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar estado",
                    status = 200,
                    message = "Estado actualizado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Editar estado " + ex.Message.ToString() + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Editar estado",
                    status = 400,
                    message = ""
                }); ;
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarEstados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarEstados eliminarEstados)
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
                        title = "Eliminar estado",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estados = await context.estados.ToListAsync();

                //Consulta de estados
                var existeEstado = context.estados.Where(x => x.Id.Equals(eliminarEstados.Id)).FirstOrDefault();

                if (existeEstado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar estados",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                context.Remove(existeEstado);

                //Se elimina el regitro
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Eliminar estado",
                    status = 200,
                    message = "Estado eliminado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar estado " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Eliminar estado",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
