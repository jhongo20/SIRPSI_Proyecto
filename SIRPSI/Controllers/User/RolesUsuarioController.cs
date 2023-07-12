using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Rols;
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
using SIRPSI.DTOs.User.Roles;
using SIRPSI.DTOs.User.RolesUsuario;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.User
{
    [Route("api/rolesusuario")]
    [ApiController]
    [EnableCors("CorsApi")]
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

                //Consulta de usuario
                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar roles usuario",
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
                        title = "Consultar roles usuario",
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
                    var estados = await context.estados.ToListAsync();
                    var estado = estados.Where(x => x.IdConsecutivo.Equals(1)).Select(x => x.Id).First();

                    //Consulta el rol
                    var rol = await context.AspNetUserRoles
                        .Join(context.Roles,
                        ru => ru.RoleId,
                        r => r.Id,
                        (ru, r) => new { rolUsuario = ru, roles = r }).                       
                        Where(x => x.rolUsuario.IdEstado.Equals(estado)).Select(x => new
                    {
                        x.rolUsuario.UserId,
                        x.rolUsuario.RoleId,
                        x.rolUsuario.IdEstado,
                        x.roles.Name

                    }).ToListAsync();

                    if (rol == null)
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
                    return rol;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar roles usuario",
                        status = 400,
                        message = "No tiene permisos para consultar roles usuario"
                    });
                }

            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar roles usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
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
                        title = "Registrar roles usuario",
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
                        title = "Registrar roles usuario",
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
                    var estados = await context.estados.ToListAsync();

                    //Mapeo de datos en clases
                    var rol = mapper.Map<UserRoles>(registrarRolesUsuario);

                    //Valores asignados
                    rol.Id = Guid.NewGuid().ToString();
                    rol.UserId = registrarRolesUsuario.UserId != null ? registrarRolesUsuario.UserId : "";
                    rol.RoleId = registrarRolesUsuario.RoleId != null ? registrarRolesUsuario.RoleId : "";
                    rol.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(1)).Select(x => x.Id).First();
                    rol.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                    rol.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                    rol.UsuarioModifico = null;
                    rol.UsuarioModifico = null;
                    rol.Discriminator = "UserRoles";

                    //Agregar datos al contexto
                    context.Add(rol);
                    //Guardado de datos 
                    await context.SaveChangesAsync();

                    return Created("", new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Registrar roles usuario",
                        status = 201,
                        message = "Rol creado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar roles usuario",
                        status = 400,
                        message = "No tiene permisos para registrar roles usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar roles usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar roles usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Actualizar
        [HttpPut("ActualizarRolesUsuario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarRolesUsuario actualizarRolesUsuario)
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
                        title = "Actualizar roles usuario",
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
                        title = "Actualizar roles usuario",
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

                    //Consulta de roles de usuario
                    var existe = await context.AspNetRoles.Where(x => x.Id.Equals(actualizarRolesUsuario.RoleId)).FirstOrDefaultAsync();

                    if (existe == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar roles usuario",
                            status = 404,
                            message = "Rol usuario no encontrado"
                        });
                    }

                    //Consulta de estados
                    var estado = await context.estados.Where(x => x.Id.Equals(actualizarRolesUsuario.IdEstado)).FirstOrDefaultAsync();

                    if (estado == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar roles usuario",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Registro de datos
                    context.AspNetUserRoles.Where(x => x.UserId.Equals(existe.Id)).ToList()
                        .ForEach(r =>
                        {
                            r.UserId = actualizarRolesUsuario.UserId != null ? actualizarRolesUsuario.UserId : "";
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
                        title = "Actualizar roles usuario",
                        status = 200,
                        message = "Rol usuario actualizado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar roles usuario",
                        status = 400,
                        message = "No tiene permisos para actualizar roles usuario"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Actualizar roles usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar roles usuario",
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
                        title = "Eliminar roles usuarios",
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
                        title = "Eliminar roles usuarios",
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

                    //Consulta de estados
                    var estados = await context.estados.ToListAsync();

                    if (estados == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Eliminar roles usuarios",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Agregar datos al contexto
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
                        title = "Eliminar roles usuarios",
                        status = 200,
                        message = "Rol eliminado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar roles usuarios",
                        status = 400,
                        message = "No tiene permisos para eliminar roles usuarios"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar roles usuarios " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Eliminar roles usuarios",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion
    }
}
