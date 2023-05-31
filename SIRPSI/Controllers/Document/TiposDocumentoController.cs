using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Companies;
using DataAccess.Models.Documents;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.Companies;
using SIRPSI.DTOs.Document;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.Document
{
    [Route("api/tipodocumento")]
    [ApiController]
    public class TiposDocumentoController : ControllerBase
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
        public TiposDocumentoController(AppDbContext context,
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
        [HttpGet("ConsultarTipoDocumento", Name = "consultarTipoDocumento")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get([FromBody] ConsultarTiposDocumento consultarTiposDocumento)
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
                        title = "Registrar tipo documento",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Consultar estados
                var estado = await context.estados.Where(x => x.Id.Equals(consultarTiposDocumento.IdEstado)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Estados tipo documento",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Consulta el tipo documento
                var tipoEmpresa = context.tiposDocumento.Where(x => x.IdEstado.Equals(estado.Id)).Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Descripcion,
                    x.IdEstado

                }).ToList();

                if (tipoEmpresa == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar tipo documento",
                        status = 404,
                        message = "Tipo documento no encontrada"
                    });
                }

                //Retorno de los datos encontrados
                return tipoEmpresa;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar tipo documento" + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Consultar tipo documento",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }

        #endregion

        #region Registro
        [HttpPost("RegistrarTipoDocumento")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarTipoDocumento registrarTipoDocumento)
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
                        title = "Registrar tipo documento",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estado = await context.estados.Where(x => x.Id.Equals(registrarTipoDocumento.IdEstado)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar tipo documento",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Mapeo de datos en clases
                var tipoEmpresa = mapper.Map<TiposDocumento>(registrarTipoDocumento);
                //Valores asignados
                tipoEmpresa.Id = Guid.NewGuid().ToString();
                tipoEmpresa.Nombre = registrarTipoDocumento.Nombre != null ? registrarTipoDocumento.Nombre : "";
                tipoEmpresa.Descripcion = registrarTipoDocumento.Descripcion;
                tipoEmpresa.IdEstado = estado.Id;
                tipoEmpresa.UsuarioRegistro = usuario.Document != null ? usuario.Document : ""; ;
                tipoEmpresa.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                tipoEmpresa.FechaModifico = null;
                tipoEmpresa.UsuarioModifico = null;

                //Agregar datos al contexto
                context.Add(tipoEmpresa);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar tipo documento",
                    status = 201,
                    message = "Tipo documento creado"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar tipo documento " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar tipo documento",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarTipoDocumento")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarTipoDocumento editarTipoDocumento)
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
                        title = "Editar tipo documento",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de empresa del usuario
                var existe = await context.tiposDocumento.Where(x => x.Id.Equals(editarTipoDocumento.Id)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar tipo documento",
                        status = 404,
                        message = "Tipo documento no encontrado"
                    });
                }

                //Registro de datos
                context.tiposDocumento.Where(x => x.Id.Equals(existe.Id)).ToList()
                    .ForEach(r =>
                    {
                        r.Nombre = editarTipoDocumento.Nombre;
                        r.Descripcion = editarTipoDocumento.Descripcion;
                        r.UsuarioModifico = usuario.Document;
                        r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                    });
                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar tipo documento",
                    status = 200,
                    message = "Tipo documento actualizado"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar tipo documento" + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar tipo documento",
                    status = 400,
                    message = ""
                }); ;
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarTipoDocumento")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarTipoDocumento eliminarTipoDocumento)
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
                        title = "Eliminar tipo documento",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta estados
                var estados = await context.estados.ToListAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar tipo documento",
                        status = 404,
                        message = "Estados no encontrados"
                    });
                }

                //Consulta de empresa
                var existe = await context.tiposDocumento.Where(x => x.Id.Equals(eliminarTipoDocumento.Id)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar tipo documento",
                        status = 404,
                        message = "Tipo empresa no encontrada"
                    });
                }

                //Agregar datos al contexto
                context.tiposDocumento.Where(x => x.Id.Equals(eliminarTipoDocumento.Id)).ToList()
                  .ForEach(r =>
                  {
                      r.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
                      r.UsuarioModifico = usuario.Document;
                      r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                  });

                //Se elimina el regitro de forma logica
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Eliminar tipo documento",
                    status = 200,
                    message = "Tipo documento eliminado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar tipo documento " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Eliminar tipo documento",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
