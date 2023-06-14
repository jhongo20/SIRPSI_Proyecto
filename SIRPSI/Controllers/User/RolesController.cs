using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
using DataAccess.Models.Rols;
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
                        title = "Consultar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Obtiene la url del servicio
                string getUrl = HttpContext.Request.GetDisplayUrl();

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
                        title = "Consultar roles",
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

                    //Consulta estado
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
                    var rol = context.AspNetRoles.Where(x => x.Status.Equals(estado.Id)).Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.Status

                    }).ToList();

                    if (rol == null)
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
                    return rol;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar roles",
                        status = 400,
                        message = "No tiene permisos para consultar roles"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar roles " + ex.Message.ToString() + " - " + ex.StackTrace);
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
                        title = "Registrar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Obtiene la url del servicio
                string getUrl = HttpContext.Request.GetDisplayUrl();

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
                        title = "Registrar roles",
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
                    var rol = mapper.Map<Roles>(registrarRoles);
                    //Valores asignados
                    rol.Id = Guid.NewGuid().ToString();
                    rol.Name = registrarRoles.Name;
                    rol.Status = estado.Id;
                    rol.ConcurrencyStamp = Guid.NewGuid().ToString();
                    rol.Description = registrarRoles.Description;
                    rol.UserRegistration = usuario.Document;
                    rol.RegistrationDate = DateTime.Now.ToDateTimeZone().DateTime;
                    rol.UserModify = null;
                    rol.ModifiedDate = null;

                    //Agregar datos al contexto
                    context.Add(rol);
                    //Guardado de datos 
                    await context.SaveChangesAsync();

                    return Created("", new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Registrar roles",
                        status = 201,
                        message = "Rol creado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar roles",
                        status = 400,
                        message = "No tiene permisos para registrar roles"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar roles " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar roles",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Actualizar
        [HttpPut("ActualizarRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarRol actualizarRol)
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
                        title = "Actualizar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Obtiene la url del servicio
                string getUrl = HttpContext.Request.GetDisplayUrl();

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
                        title = "Actualizar roles",
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
                    var existeRol = await context.AspNetRoles.Where(x => x.Id.Equals(actualizarRol.Id)).FirstOrDefaultAsync();

                    if (existeRol == null)
                    {
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar roles",
                            status = 404,
                            message = "Rol no encontrado"
                        });
                    }

                    //Consulta de estados
                    var estados = await context.estados.ToListAsync();

                    //Registro de datos
                    context.AspNetRoles.Where(x => x.Id.Equals(actualizarRol.Id)).ToList()
                        .ForEach(r =>
                        {
                            r.Name = actualizarRol.Name;
                            r.Description = actualizarRol.Description;
                            r.UserModify = usuario.Document;
                            r.ModifiedDate = DateTime.Now.ToDateTimeZone().DateTime;
                        });

                    //Guardado de datos
                    await context.SaveChangesAsync();

                    return Ok(new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Actualizar roles",
                        status = 200,
                        message = "Rol actualizado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar roles",
                        status = 400,
                        message = "No tiene permisos para actualizar roles"
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Actualizar roles " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar roles",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
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
                        title = "Eliminar roles",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }


                //Obtiene la url del servicio
                string getUrl = HttpContext.Request.GetDisplayUrl();

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
                        title = "Eliminar roles",
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

                    //Agrega datos al contexto
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
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar roles",
                        status = 400,
                        message = "No tiene permisos para eliminar roles"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar roles " + ex.Message.ToString() + " - " + ex.StackTrace);
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
