using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Country;
using DataAccess.Models.Status;
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
using SIRPSI.DTOs.Country;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.Country
{
    [Route("api/pais")]
    [ApiController]
    public class PaisesController : ControllerBase
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
        public PaisesController(AppDbContext context,
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
        [HttpGet("ConsultarPaises", Name = "consultarPaises")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get([FromBody] ConsultarPaises consultarPaises)
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
                        title = "Registrar país",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Consulta estados
                var estados = await context.estados.Where(x => x.Id.Equals(consultarPaises.IdEstado)).ToListAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar país",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                //Consulta el país
                var pais = context.pais.Where(x => x.IdEstado.Equals(consultarPaises.IdEstado)).Select(x => new
                {
                    x.Id,    
                    x.Nombre,
                    x.Descripcion,
                    x.IdEstado

                }).ToList();

                if (pais == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar país",
                        status = 404,
                        message = "Países no encontrados"
                    });
                }

                //Retorno de los datos encontrados
                return pais;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar país " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Consultar país",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }

        #endregion

        #region Registro
        [HttpPost("RegistrarPais")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarPais registrarPais)
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
                        title = "Registrar pais",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta estados
                var estados = await context.estados.Where(x => x.Id.Equals(registrarPais.IdEstado)).FirstOrDefaultAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar pais",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                //Mapeo de datos en clases
                var pais = mapper.Map<Pais>(registrarPais);
                //Valores asignados
                pais.Id = Guid.NewGuid().ToString();
                pais.Nombre = registrarPais.Nombre != null ? registrarPais.Nombre : "";
                pais.Descripcion = registrarPais.Descripcion;
                pais.IdEstado = estados.Id;
                pais.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                pais.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                pais.FechaModifico = null;
                pais.UsuarioModifico = null;

                //Agregar datos al contexto
                context.Add(pais);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar pais",
                    status = 201,
                    message = "Pais creado"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar pais " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar pais",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarPais")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarPais editarPais)
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
                        title = "Editar país",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de país del usuario
                var existePais = await context.pais.Where(x => x.Id.Equals(editarPais.Id)).FirstOrDefaultAsync();

                if (existePais == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar país",
                        status = 404,
                        message = "País no encontrad0"
                    });
                }

                //Registro de datos
                context.pais.Where(x => x.Id.Equals(existePais.Id)).ToList()
                    .ForEach(r =>
                    {
                        r.Nombre = editarPais.Nombre;
                        r.Descripcion = editarPais.Descripcion;
                        r.UsuarioModifico = usuario.Document;
                        r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                    });
                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar país",
                    status = 200,
                    message = "País actualizado"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar país " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar país",
                    status = 400,
                    message = ""
                }); ;
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarPais")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarPais eliminarPais)
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
                        title = "Eliminar país",
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
                        title = "Eliminar país",
                        status = 404,
                        message = "Estados no encontrado"
                    });
                }

                //Consulta de país
                var existePais = await context.pais.Where(x => x.Id.Equals(eliminarPais.Id)).FirstOrDefaultAsync();

                if (existePais == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar país",
                        status = 404,
                        message = "País no encontrado"
                    });
                }

                //Agregar datos al contexto
                context.pais.Where(x => x.Id.Equals(eliminarPais.Id)).ToList()
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
                    title = "Eliminar país",
                    status = 200,
                    message = "país eliminado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar país " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Eliminar país",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
