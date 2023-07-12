using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Documents;
using DataAccess.Models.Estados;
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
using SIRPSI.DTOs.Document;
using SIRPSI.DTOs.User.Usuario;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.User
{
    [Route("api/usuario")]
    [ApiController]
    public class UsuariosController : ControllerBase
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
        public UsuariosController(AppDbContext context,
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
        [HttpGet("ConsultarUsuarios", Name = "consultarUsuarios")]
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

                var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar tipo documento",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //consulta estados
                var estados = await context.estados.ToListAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar usuario",
                        status = 404,
                        message = "Estados no encontrados"
                    });
                }

                var estado = await context.estados.Where(x => x.IdConsecutivo.Equals(1)).FirstOrDefaultAsync();

                if (!usuario.Status.Equals(estado.Id))
                {
                    return NotFound(new General()
                    {
                        title = "Consultar usuario",
                        status = 404,
                        message = "Usuario no activo en el sistema"
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
                        title = "Consultar tipo documento",
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
                    //Consulta el tipo documento
                    var usuarioConsultado = await context.AspNetUsers
                        .Join(context.estados,
                        u => u.Status,
                        e => e.Id,
                        (u, e) => new { usuario = u, estado = e })
                        .Join(context.Roles,
                        ru => ru.usuario.IdRol,
                        r => r.Id,
                        (ru, r) => new { rolUsuario = ru, roles = r })
                        .Join(context.empresas,
                        eu => eu.rolUsuario.usuario.IdCompany,
                        em => em.Id,
                        (eu,em) => new  {usuarioEm = eu, empresa = em })
                        .Join(context.tiposDocumento,
                        tu => tu.usuarioEm.rolUsuario.usuario.TypeDocument,
                        td => td.Id,
                        (tu, td) => new { tipoDocU = tu, tipoDoc = td })                      

                        .Where(x => x.tipoDocU.usuarioEm.rolUsuario.usuario.Status.Equals(estado.Id)).Select(x => new
                        {
                            x.tipoDocU.usuarioEm.rolUsuario.usuario.Id,
                            idTipoDocumento = x.tipoDoc.Id,
                            nombreTipoDocumento = x.tipoDoc.Nombre,
                            cedula = x.tipoDocU.usuarioEm.rolUsuario.usuario.Document,
                            correo = x.tipoDocU.usuarioEm.rolUsuario.usuario.Email,
                            telefono = x.tipoDocU.usuarioEm.rolUsuario.usuario.PhoneNumber,
                            idEmpresa = x.tipoDocU.empresa.Id,
                            nombreEmpresa = x.tipoDocU.empresa.Nombre,
                            nombreUsuario = x.tipoDocU.usuarioEm.rolUsuario.usuario.Names,
                            apellidosUsuario = x.tipoDocU.usuarioEm.rolUsuario.usuario.Surnames,
                            idEstado = x.tipoDocU.usuarioEm.rolUsuario.usuario.Status,
                            nombreEstado = x.tipoDocU.usuarioEm.rolUsuario.estado.Nombre
                        }).ToListAsync();

                    if (usuarioConsultado == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new General()
                        {
                            title = "Consultar usuario",
                            status = 404,
                            message = "Usuarios no encontrados"
                        });
                    }

                    //Retorno de los datos encontrados
                    return usuarioConsultado;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar usuario",
                        status = 400,
                        message = "No tiene permisos para consultar usuarios"
                    });
                }

            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar usuario " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Consultar usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Registro
        //[HttpPost("RegistrarUsuario")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<ActionResult> Post([FromBody] RegistrarTipoDocumento registrarTipoDocumento)
        //{
        //    try
        //    {
        //        //Claims de usuario - Enviados por token
        //        var identity = HttpContext.User.Identity as ClaimsIdentity;

        //        if (identity != null)
        //        {
        //            IEnumerable<Claim> claims = identity.Claims;
        //        }

        //        //Consulta el documento con los claims
        //        var documento = identity.FindFirst("documento").Value.ToString();

        //        //Consulta el rol con los claims
        //        var roles = identity.FindFirst("rol").Value.ToString();

        //        //Consulta de usuarios por documento
        //        var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

        //        if (usuario == null)
        //        {
        //            return NotFound(new General()
        //            {
        //                title = "Registrar tipo documento",
        //                status = 404,
        //                message = "Usuario no encontrado"
        //            });
        //        }

        //        //Consultar estados
        //        var estado = await context.estados.Where(x => x.Id.Equals(registrarTipoDocumento.IdEstado)).FirstOrDefaultAsync();

        //        if (estado == null)
        //        {
        //            return NotFound(new General()
        //            {
        //                title = "Registrar tipo documento",
        //                status = 404,
        //                message = "Estado no encontrado"
        //            });
        //        }

        //        //Obtiene la url del servicio
        //        string getUrl = HttpContext.Request.GetDisplayUrl();

        //        //Consulta de roles por id de usuario

        //        var rolesList = new List<string>();

        //        //Verifica los roles
        //        var list = roles.Split(',').ToList();

        //        foreach (var i in list)
        //        {
        //            var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

        //            if (result != null)
        //            {
        //                rolesList.Add(result.ToString());
        //            }
        //        }

        //        if (rolesList == null)
        //        {
        //            return NotFound(new General()
        //            {
        //                title = "Registrar tipo documento",
        //                status = 404,
        //                message = "Roles no encontrados"
        //            });
        //        }

        //        //Revisa los permisos de usuario
        //        var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

        //        //Consulta si tiene el permiso
        //        var permitido = permisos.Select(x => x.Registrar.Equals(true)).FirstOrDefault();

        //        //Si es permitido
        //        if (permitido == true)
        //        {
        //            //Mapeo de datos en clases
        //            var tipoEmpresa = mapper.Map<TiposDocumento>(registrarTipoDocumento);
        //            //Valores asignados
        //            tipoEmpresa.Id = Guid.NewGuid().ToString();
        //            tipoEmpresa.Nombre = registrarTipoDocumento.Nombre != null ? registrarTipoDocumento.Nombre : "";
        //            tipoEmpresa.Descripcion = registrarTipoDocumento.Descripcion;
        //            tipoEmpresa.IdEstado = estado.Id;
        //            tipoEmpresa.UsuarioRegistro = usuario.Document != null ? usuario.Document : ""; ;
        //            tipoEmpresa.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
        //            tipoEmpresa.FechaModifico = null;
        //            tipoEmpresa.UsuarioModifico = null;

        //            //Agregar datos al contexto
        //            context.Add(tipoEmpresa);
        //            //Guardado de datos 
        //            await context.SaveChangesAsync();

        //            return Created("", new General()
        //            {
        //                //Visualizacion de mensajes al usuario del aplicativo
        //                title = "Registrar tipo documento",
        //                status = 201,
        //                message = "Tipo documento creado"
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest(new General()
        //            {
        //                title = "Registrar tipo documento",
        //                status = 400,
        //                message = "No tiene permisos para registrar tipo documento"
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Registro de errores
        //        logger.LogError("Registrar tipo documento " + ex.Message.ToString() + " - " + ex.StackTrace);
        //        return BadRequest(new General()
        //        {
        //            title = "Registrar tipo documento",
        //            status = 400,
        //            message = "Contacte con el administrador del sistema"
        //        });
        //    }
        //}
        #endregion

 

        #region Eliminar
        //[HttpDelete("EliminarUusario")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<ActionResult> Delete([FromBody] EliminarTipoDocumento eliminarTipoDocumento)
        //{
        //    try
        //    {
        //        //Claims de usuario - Enviados por token
        //        var identity = HttpContext.User.Identity as ClaimsIdentity;

        //        if (identity != null)
        //        {
        //            IEnumerable<Claim> claims = identity.Claims;
        //        }

        //        //Consulta el documento con los claims
        //        var documento = identity.FindFirst("documento").Value.ToString();

        //        //Consulta el rol con los claims
        //        var roles = identity.FindFirst("rol").Value.ToString();

        //        //Consulta de usuario
        //        var usuario = context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefault();

        //        if (usuario == null)
        //        {
        //            return NotFound(new General()
        //            {
        //                title = "Eliminar tipo documento",
        //                status = 404,
        //                message = "Usuario no encontrado"
        //            });
        //        }

        //        //Obtiene la url del servicio
        //        string getUrl = HttpContext.Request.GetDisplayUrl();

        //        //Consulta de roles por id de usuario

        //        var rolesList = new List<string>();

        //        //Verifica los roles
        //        var list = roles.Split(',').ToList();

        //        foreach (var i in list)
        //        {
        //            var result = context.AspNetRoles.Where(r => r.Id.Equals(i)).Select(x => x.Description).FirstOrDefault();

        //            if (result != null)
        //            {
        //                rolesList.Add(result.ToString());
        //            }
        //        }

        //        if (rolesList == null)
        //        {
        //            return NotFound(new General()
        //            {
        //                title = "Eliminar tipo documento",
        //                status = 404,
        //                message = "Roles no encontrados"
        //            });
        //        }

        //        //Revisa los permisos de usuario
        //        var permisos = await context.permisosXUsuario.Where(x => x.Vista.Equals(getUrl) && x.IdUsuario.Equals(usuario.Id)).ToListAsync();

        //        //Consulta si tiene el permiso
        //        var permitido = permisos.Select(x => x.Eliminar.Equals(true)).FirstOrDefault();

        //        //Si es permitido
        //        if (permitido == true)
        //        {
        //            //Consulta estados
        //            var estados = await context.estados.ToListAsync();

        //            if (estados == null)
        //            {
        //                return NotFound(new General()
        //                {
        //                    title = "Eliminar tipo documento",
        //                    status = 404,
        //                    message = "Estados no encontrados"
        //                });
        //            }

        //            //Consulta de empresa
        //            var existe = await context.tiposDocumento.Where(x => x.Id.Equals(eliminarTipoDocumento.Id)).FirstOrDefaultAsync();

        //            if (existe == null)
        //            {
        //                return NotFound(new General()
        //                {
        //                    title = "Eliminar tipo documento",
        //                    status = 404,
        //                    message = "Tipo empresa no encontrada"
        //                });
        //            }

        //            //Agregar datos al contexto
        //            context.tiposDocumento.Where(x => x.Id.Equals(eliminarTipoDocumento.Id)).ToList()
        //              .ForEach(r =>
        //              {
        //                  r.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
        //                  r.UsuarioModifico = usuario.Document;
        //                  r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
        //              });

        //            //Se elimina el regitro de forma logica
        //            await context.SaveChangesAsync();

        //            return Ok(new General()
        //            {
        //                //Visualizacion de mensajes al usuario del aplicativo
        //                title = "Eliminar tipo documento",
        //                status = 200,
        //                message = "Tipo documento eliminado"
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest(new General()
        //            {
        //                title = "Eliminar tipo documento",
        //                status = 400,
        //                message = "No tiene permisos para eliminar tipo documento"
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Registro de errores
        //        logger.LogError("Eliminar tipo documento " + ex.Message.ToString() + " - " + ex.StackTrace);
        //        return BadRequest(new General()
        //        {
        //            title = "Eliminar tipo documento",
        //            status = 400,
        //            message = "Contacte con el administrador del sistema"
        //        });
        //    }

        //}
        #endregion
    }
}
