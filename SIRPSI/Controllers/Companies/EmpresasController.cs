using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Rols;
using DataAccess.Models.Status;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.Companies;
using SIRPSI.DTOs.Document;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.Companies
{
    [Route("api/empresas")]
    [ApiController]
    public class EmpresasController : ControllerBase
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
        public EmpresasController(AppDbContext context,
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
        [HttpGet("ConsultarEmpresas", Name = "consultarEmpresas")]
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
                var usuario = await context.AspNetUsers.Where(u => u.Document.Equals(documento)).FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound(new General()
                    {
                        title = "Consultar empresas",
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
                        title = "Registrar empresas",
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
                            title = "Consultar empresas",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Consulta el empresa
                    var empresa = context.empresas.Where(x => x.IdEstado.Equals(estado.Id)).Select(x => new
                    {
                        x.Id,
                        x.TipoDocumento,
                        x.Documento,
                        x.DigitoVerificacion,
                        x.IdTipoEmpresa,
                        x.Nombre,
                        x.IdMinisterio,
                        x.IdEstado,
                        x.FechaRegistro,
                        x.FechaModifico

                    }).ToList();

                    if (empresa == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new General()
                        {
                            title = "Consultar empresas",
                            status = 404,
                            message = "Empresa no encontrada"
                        });
                    }

                    //Retorno de los datos encontrados
                    return empresa;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar empresa",
                        status = 400,
                        message = "No tiene permisos para consultar empresas"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar empresa " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Consultar empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }

        #endregion

        #region Registro
        [HttpPost("RegistrarEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarEmpresas registrarEmpresas)
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
                        title = "Registrar empresa",
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
                        title = "Registrar empresas",
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
                    var estados = await context.estados.Where(x => x.Id.Equals(registrarEmpresas.IdEstado)).FirstOrDefaultAsync();

                    if (estados == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Registrar empresa",
                            status = 404,
                            message = "Estado no encontrado"
                        });
                    }

                    //Consulta tipo documento
                    var tipoDocumento = await context.tiposDocumento.Where(x => x.Id.Equals(registrarEmpresas.TipoDocumento)).FirstOrDefaultAsync();

                    if (tipoDocumento == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Registrar empresa",
                            status = 404,
                            message = "Tipo documento no encontrado"
                        });
                    }

                    //Consulta tipo empresa
                    var tipoEmpresa = await context.tiposEmpresas.Where(x => x.Id.Equals(registrarEmpresas.IdTipoEmpresa)).FirstOrDefaultAsync();

                    if (tipoEmpresa == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Registrar empresa",
                            status = 404,
                            message = "Tipo empresa no encontrado"
                        });
                    }

                    //Mapeo de datos en clases
                    var empresa = mapper.Map<Empresas>(registrarEmpresas);
                    //Valores asignados
                    empresa.Id = Guid.NewGuid().ToString();
                    empresa.TipoDocumento = registrarEmpresas.TipoDocumento;
                    empresa.Documento = registrarEmpresas.Documento;
                    empresa.Nombre = registrarEmpresas.Nombre;
                    empresa.Descripcion = registrarEmpresas.Descripcion;
                    empresa.IdTipoEmpresa = registrarEmpresas.IdTipoEmpresa;
                    empresa.DigitoVerificacion = registrarEmpresas.DigitoVerificacion;
                    empresa.IdEstado = estados.Id;
                    empresa.UsuarioRegistro = usuario.Document;
                    empresa.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                    empresa.FechaModifico = null;
                    empresa.UsuarioModifico = null;

                    //Agregar datos al contexto
                    context.Add(empresa);
                    //Guardado de datos 
                    await context.SaveChangesAsync();

                    return Created("", new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Registrar empresa",
                        status = 201,
                        message = "Empresa creada"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar empresa",
                        status = 400,
                        message = "No tiene permisos para registrar empresas"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar empresa " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Actualizar
        [HttpPut("ActualizarEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(ActualizarEmpresas actualizarEmpresas)
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
                        title = "Actualizar empresa",
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
                    //Consulta tipo documento
                    var tipoDocumento = await context.tiposDocumento.Where(x => x.Id.Equals(actualizarEmpresas.TipoDocumento)).FirstOrDefaultAsync();

                    if (tipoDocumento == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Actualizar empresa",
                            status = 404,
                            message = "Tipo documento no encontrado"
                        });
                    }

                    //Consulta tipo empresa
                    var tipoEmpresa = await context.tiposEmpresas.Where(x => x.Id.Equals(actualizarEmpresas.IdTipoEmpresa)).FirstOrDefaultAsync();

                    if (tipoEmpresa == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Actualizar empresa",
                            status = 404,
                            message = "Tipo empresa no encontrado"
                        });
                    }

                    //Consulta de empresa del usuario
                    var existe = await context.empresas.Where(x => x.Id.Equals(actualizarEmpresas.Id)).FirstOrDefaultAsync();

                    if (existe == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new
                        {
                            //Visualizacion de mensajes al usuario del aplicativo
                            title = "Actualizar empresa",
                            status = 404,
                            message = "Empresa no encontrada"
                        });
                    }
                    //Consulta de estados
                    var estados = await context.estados.ToListAsync();
                    //Registro de datos
                    context.empresas.Where(x => x.Id.Equals(existe.Id)).ToList()
                        .ForEach(r =>
                        {
                            r.TipoDocumento = actualizarEmpresas.TipoDocumento;
                            r.Documento = actualizarEmpresas.Documento != null ? actualizarEmpresas.Documento : "";
                            r.Nombre = actualizarEmpresas.Nombre != null ? actualizarEmpresas.Nombre : "";
                            r.Descripcion = actualizarEmpresas.Descripcion != null ? actualizarEmpresas.Descripcion : "";
                            r.IdTipoEmpresa = actualizarEmpresas.IdTipoEmpresa;
                            r.DigitoVerificacion = actualizarEmpresas.DigitoVerificacion;
                            r.UsuarioModifico = usuario.Document;
                            r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                        });
                    //Guardado de datos
                    await context.SaveChangesAsync();

                    return Ok(new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Actualizar empresa",
                        status = 200,
                        message = "Empresa actualizada"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Actualizar empresa",
                        status = 400,
                        message = "No tiene permisos para actualizar empresas"
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Actualizar empresa " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Actualizar empresa",
                    status = 400,
                    message = ""
                }); 
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarEmpresas eliminarEmpresas)
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
                        title = "Eliminar empresa",
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
                        title = "Eliminar empresa",
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
                            title = "Eliminar empresa",
                            status = 404,
                            message = "Estados no encontrado"
                        });
                    }

                    //Consulta de empresa
                    var existe = await context.empresas.Where(x => x.Id.Equals(eliminarEmpresas.Id)).FirstOrDefaultAsync();

                    if (existe == null)
                    {
                        return NotFound(new General()
                        {
                            title = "Eliminar empresa",
                            status = 404,
                            message = "Empresa no encontrada"
                        });
                    }

                    //Agregar datos al contexto
                    context.empresas.Where(x => x.Id.Equals(eliminarEmpresas.Id)).ToList()
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
                        title = "Eliminar empresa",
                        status = 200,
                        message = "Empresa eliminada"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Eliminar empresa",
                        status = 400,
                        message = "No tiene permisos para eliminar empresas"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar empresa " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Eliminar empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
