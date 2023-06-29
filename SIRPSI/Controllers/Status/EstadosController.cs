using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
    [EnableCors("CorsApi")]
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
                        title = "Consultar estados",
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
                        title = "Consultar estados",
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
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar estados",
                        status = 400,
                        message = "No tiene permisos para consultar estados"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar estados " + ex.Message.ToString() + " - " + ex.StackTrace);
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

                //Consulta el documento con los claims
                var documento = identity.FindFirst("documento").Value.ToString();

                //Consulta el rol con los claims
                var roles = identity.FindFirst("rol").Value.ToString();

                //Consulta de usuarios
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
                        title = "Registrar estados",
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
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar estado",
                        status = 400,
                        message = "No tiene permisos para registrar estados"
                    });
                }
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

        #region Actualizar
        [HttpPut("ActualizarEstados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarEstados actualizarEstados)
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
                        title = "Actualizar estado",
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
                        title = "Actualizar estado",
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
                    //Consulta de estados de usuario
                    var existeEstado = await context.estados.Where(x => x.Id.Equals(actualizarEstados.Id)).FirstOrDefaultAsync();

                    if (existeEstado == null)
                    {
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar estado",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Registro de datos
                    context.estados.Where(x => x.Id.Equals(existeEstado.Id)).ToList()
                        .ForEach(e =>
                        {
                            e.Nombre = actualizarEstados.Nombre != null ? actualizarEstados.Nombre : "";
                            e.Descripcion = actualizarEstados.Descripcion;
                            e.UsuarioModifico = usuario.Document;
                            e.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                        });

                    //Guardado de datos
                    await context.SaveChangesAsync();

                    return Ok(new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Actualizar estado",
                        status = 200,
                        message = "Estado actualizado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar estado",
                        status = 400,
                        message = "No tiene permisos para actualizar estado"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Actualizar estado " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar estado",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
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
                        title = "Eliminar estado",
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
                        title = "Eliminar estados",
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
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar estado",
                        status = 400,
                        message = "No tiene permisos para eliminar estados"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar estado " + ex.Message.ToString() + " - " + ex.StackTrace);
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
