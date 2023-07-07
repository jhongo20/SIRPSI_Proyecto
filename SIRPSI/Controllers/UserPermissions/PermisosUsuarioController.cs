using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
using DataAccess.Models.Permissions;
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
using SIRPSI.DTOs.Document;
using SIRPSI.DTOs.Status;
using SIRPSI.DTOs.UserPermissions;
using SIRPSI.Helpers.Answers;
using System.IO;
using System.Security.Claims;

namespace SIRPSI.Controllers.UserPermissions
{
    [Route("api/permisosusuario")]
    [ApiController]
    [EnableCors("CorsApi")]
    public class PermisosUsuarioController : ControllerBase
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
        public PermisosUsuarioController(AppDbContext context,
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
        [HttpGet("ConsultarPermisosUsuario", Name = "consultarPermisosUsuario")]
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
                        title = "Consultar permisos usuario",
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
                        title = "Consultar permisos usuario",
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

                    var estados = await context.estados.Select(x => new { x.Id, x.IdConsecutivo }).Where(x => x.IdConsecutivo.Equals(1) || x.IdConsecutivo.Equals(2)).Select(x => x.Id).ToListAsync();

                    //Consulta el permisos
                    var permisosUsuario = await context.permisosXUsuario.Where(x => x.IdUsuario.Equals(usuario.Id) && estados.Contains(x.IdEstado)).Select(x => new ConsultarPermisosUsuario
                    {
                        Id = x.Id,
                        Vista = x.Vista,
                        IdUsuario = x.IdUsuario,
                        IdEmpresa = x.IdEmpresa,
                        Consulta = x.Consulta,
                        Registrar = x.Registrar,
                        Actualizar = x.Actualizar,
                        Reportes = x.Reportes,
                        Eliminar = x.Eliminar,
                        IdEstado = x.IdEstado
                    }).ToListAsync();

                    if (permisosUsuario == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new General()
                        {
                            title = "Consultar permisos usuario",
                            status = 404,
                            message = "Permisos no encontrados"
                        });
                    }

                    //Retorno de los datos encontrados
                    return permisosUsuario;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar permisos usuario",
                        status = 400,
                        message = "No tiene permisos para consultar permisos por usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar permisos usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Consultar permisos usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Registro
        [HttpPost("RegistrarPermisosUsuario", Name = "registrarPermisosUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarPermisosUsuario registrarPermisosUsuario)
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
                        title = "Registrar permisos usuario",
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
                        title = "Registrar permisos usuario",
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
                    var permisosUsuario = mapper.Map<PermisosXUsuario>(registrarPermisosUsuario);

                    //Valores asignados
                    permisosUsuario.Id = Guid.NewGuid().ToString();
                    permisosUsuario.Vista = registrarPermisosUsuario.Vista;
                    permisosUsuario.IdUsuario = registrarPermisosUsuario.IdUsuario;
                    permisosUsuario.IdEmpresa = registrarPermisosUsuario.IdEmpresa;
                    permisosUsuario.Consulta = registrarPermisosUsuario.Consulta;
                    permisosUsuario.Actualizar = registrarPermisosUsuario.Actualizar;
                    permisosUsuario.Registrar = registrarPermisosUsuario.Registrar;
                    permisosUsuario.Eliminar = registrarPermisosUsuario.Eliminar;
                    permisosUsuario.IdEstado = registrarPermisosUsuario.IdEstado;
                    permisosUsuario.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                    permisosUsuario.UsuarioRegistro = usuario.Document;
 
                    //Agregar datos al contexto
                    context.Add(permisosUsuario);
                    //Guardado de datos 
                        await context.SaveChangesAsync();

                    return Created("", new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Registrar permisos usuario",
                        status = 201,
                        message = "Permisos usuario creado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar permisos usuario",
                        status = 400,
                        message = "No tiene permisos para registrar permisos por usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar permisos usuario" + ex.Message.ToString() + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar permisos usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Actualizar
        [HttpPut("ActualizarPermisosUsuario", Name = "actualizarPermisosUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarPermisosUsuario actualizarPermisosUsuario)
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
                        title = "Actualizar permisos usuario",
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
                        title = "Actualizar permisos usuario",
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
                    var existeEstado = await context.estados.Where(x => x.Id.Equals(actualizarPermisosUsuario.IdEstado)).FirstOrDefaultAsync();

                    if (existeEstado == null)
                    {
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar permisos usuario",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Registro de datos
                    context.permisosXUsuario.Where(x => x.Id.Equals(actualizarPermisosUsuario.Id)).ToList()
                        .ForEach(e =>
                        {
                            e.Vista = actualizarPermisosUsuario.Vista;
                            e.IdUsuario = actualizarPermisosUsuario.IdUsuario;
                            e.IdEmpresa = actualizarPermisosUsuario.IdEmpresa;
                            e.Consulta = actualizarPermisosUsuario.Consulta;
                            e.Actualizar = actualizarPermisosUsuario.Actualizar;
                            e.Registrar = actualizarPermisosUsuario.Registrar;
                            e.Eliminar = actualizarPermisosUsuario.Eliminar;
                            e.Reportes = actualizarPermisosUsuario.Reportes;
                            e.UsuarioModifico = usuario.Document;
                            e.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                        });

                    //Guardado de datos
                    await context.SaveChangesAsync();

                    return Ok(new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Actualizar permisos usuario",
                        status = 200,
                        message = "Permisos usuario actualizado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar permisos usuario",
                        status = 400,
                        message = "No tiene permisos para actualizar permisos por usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Actualizar permisos usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar permisos usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("eliminarPermisosUsuario", Name = "eliminarPermisosUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarPermisosUsuario eliminarPermisosUsuario)
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
                        title = "Eliminar permisos usuario",
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
                        title = "Eliminar permisos usuario",
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
                            title = "Eliminar permisos usuario",
                            status = 404,
                            message = "Estados no encontrados"
                        });
                    }

                    //Consulta de empresa
                    var existe = await context.permisosXUsuario.Where(x => x.Id.Equals(eliminarPermisosUsuario.Id)).FirstOrDefaultAsync();

                    if (existe == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Eliminar permisos usuario",
                            status = 404,
                            message = "Permisos usuario no encontrado"
                        });
                    }

                    //Agregar datos al contexto
                    context.permisosXUsuario.Where(x => x.Id.Equals(eliminarPermisosUsuario.Id)).ToList()
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
                        title = "Eliminar permisos usuario",
                        status = 200,
                        message = "Permisos usuario eliminado" 
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar permisos usuario",
                        status = 400,
                        message = "No tiene permisos para eliminar permisos de usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar permisos usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Eliminar permisos usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
