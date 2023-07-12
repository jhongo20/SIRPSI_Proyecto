using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Country;
using DataAccess.Models.Rols;
using DataAccess.Models.Status;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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

                //Consulta el documento con los claims
                var documento = identity.FindFirst("documento").Value.ToString();

                //Consulta el rol con los claims
                var roles = identity.FindFirst("rol").Value.ToString();

                //Consulta de usuarios por documento
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar país",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Obtiene la url del servicio
                var countLast = HttpContext.Request.GetDisplayUrl().Split("/").Last().Count();
                string Url = HttpContext.Request.GetDisplayUrl();

                var getUrl = Url.Remove(Url.Length - (countLast + 1));

                //Consulta de roles por id de usuario

                var rolesList = new List<string>();
                //Verifica los roles
                var list = roles.Split(',').ToList();

                foreach (var i in list)
                {
                    var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

                    if (result != null)
                    {
                        rolesList.Add(result.ToString());
                    }
                }

                if (rolesList == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar país",
                        status = 404,
                        message = "Roles no encontrados"
                    });
                }
                //Revisa los permisos de usuario
                var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

                //Consulta si tiene el permiso
                var permitido = permisos.Select(x => x.Consulta.Equals(true)).FirstOrDefault();

                //Si es permitido
                if (permitido == true)
                {

                    //Consulta estados
                    var estado = await context.estados.Where(x => x.IdConsecutivo.Equals(1)).FirstOrDefaultAsync();

                    if (estado == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Consultar país",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Consulta el país
                    var pais = context.pais.Where(x => x.IdEstado.Equals(estado.Id)).Select(x => new
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
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar país",
                        status = 400,
                        message = "No tiene permisos para consultar paises"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar país " + ex.Message.ToString() + " - " + ex.StackTrace);
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

                //Consulta el documento con los claims
                var documento = identity.FindFirst("documento").Value.ToString();

                //Consulta el rol con los claims
                var roles = identity.FindFirst("rol").Value.ToString();

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


                //Obtiene la url del servicio
                var countLast = HttpContext.Request.GetDisplayUrl().Split("/").Last().Count();
                string Url = HttpContext.Request.GetDisplayUrl();

                var getUrl = Url.Remove(Url.Length - (countLast + 1));

                //Consulta de roles por id de usuario

                var rolesList = new List<string>();

                //Verifica los roles
                var list = roles.Split(',').ToList();

                foreach (var i in list)
                {
                    var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

                    if (result != null)
                    {
                        rolesList.Add(result.ToString());
                    }
                }

                if (rolesList == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar país",
                        status = 404,
                        message = "Roles no encontrados"
                    });
                }
                //Revisa los permisos de usuario
                var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

                //Consulta si tiene el permiso
                var permitido = permisos.Select(x => x.Registrar.Equals(true)).FirstOrDefault();

                //Si es permitido
                if (permitido == true)
                {

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
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar país",
                        status = 400,
                        message = "No tiene permisos para registrar paises"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar país " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar pais",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Actualizar
        [HttpPut("ActualizarPais")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarPais actualizarPais)
        {
            try
            {
                //Claims de usuario - Enviados por token
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                }

                //Consulta el documento con los claims
                var documento = identity.FindFirst("documento").Value.ToString();

                //Consulta el rol con los claims
                var roles = identity.FindFirst("rol").Value.ToString();

                //Consulta de usuario
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Actualizar país",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Obtiene la url del servicio
                var countLast = HttpContext.Request.GetDisplayUrl().Split("/").Last().Count();
                string Url = HttpContext.Request.GetDisplayUrl();

                var getUrl = Url.Remove(Url.Length - (countLast + 1));

                //Consulta de roles por id de usuario

                var rolesList = new List<string>();

                //Verifica los roles
                var list = roles.Split(',').ToList();

                foreach (var i in list)
                {
                    var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

                    if (result != null)
                    {
                        rolesList.Add(result.ToString());
                    }
                }

                if (rolesList == null)
                {
                    return NotFound(new General()
                    {
                        title = "Actualizar empresas",
                        status = 404,
                        message = "Roles no encontrados"
                    });
                }

                //Revisa los permisos de usuario
                var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

                //Consulta si tiene el permiso
                var permitido = permisos.Select(x => x.Actualizar.Equals(true)).FirstOrDefault();

                //Si es permitido
                if (permitido == true)
                {

                    //Consulta de país del usuario
                    var existePais = await context.pais.Where(x => x.Id.Equals(actualizarPais.Id)).FirstOrDefaultAsync();

                    if (existePais == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar país",
                            status = 404,
                            message = "País no encontrad0"
                        });
                    }

                    //Registro de datos
                    context.pais.Where(x => x.Id.Equals(existePais.Id)).ToList()
                        .ForEach(r =>
                        {
                            r.Nombre = actualizarPais.Nombre;
                            r.Descripcion = actualizarPais.Descripcion;
                            r.UsuarioModifico = usuario.Document;
                            r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                        });
                    //Guardado de datos
                    await context.SaveChangesAsync();

                    return Ok(new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Actualizar país",
                        status = 200,
                        message = "País actualizado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar país",
                        status = 400,
                        message = "No tiene permisos para actualizar paises"
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Actualizar país " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar país",
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

                //Consulta el documento con los claims
                var documento = identity.FindFirst("documento").Value.ToString();

                //Consulta el rol con los claims
                var roles = identity.FindFirst("rol").Value.ToString();

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

                //Obtiene la url del servicio
                var countLast = HttpContext.Request.GetDisplayUrl().Split("/").Last().Count();
                string Url = HttpContext.Request.GetDisplayUrl();

                var getUrl = Url.Remove(Url.Length - (countLast + 1));

                //Consulta de roles por id de usuario

                var rolesList = new List<string>();

                //Verifica los roles
                var list = roles.Split(',').ToList();

                foreach (var i in list)
                {
                    var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

                    if (result != null)
                    {
                        rolesList.Add(result.ToString());
                    }
                }

                if (rolesList == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar país",
                        status = 404,
                        message = "Roles no encontrados"
                    });
                }

                //Revisa los permisos de usuario
                var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

                //Consulta si tiene el permiso
                var permitido = permisos.Select(x => x.Eliminar.Equals(true)).FirstOrDefault();

                //Si es permitido
                if (permitido == true)
                {

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
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar país",
                        status = 400,
                        message = "No tiene permisos par eliminar paises"
                    });
                }

            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar país " + ex.Message.ToString() + " - " + ex.StackTrace);
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
