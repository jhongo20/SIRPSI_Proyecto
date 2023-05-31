using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Status;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.Companies;
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
        public async Task<ActionResult<object>> Get([FromBody] ConsultarEmpresas consultarEmpresas)
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
                        title = "Registrar empresas",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Consulta estados
                var estados = await context.estados.Where(x => x.Id.Equals(consultarEmpresas.Id)).ToListAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar empresas",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }

                //Consulta el empresa
                var empresa = context.empresas.Where(x => x.IdEstado.Equals(consultarEmpresas.IdEstado)).Select(x => new
                {
                    x.Id,
                    x.TipoDocumento,
                    x.Documento,
                    x.DigitoVerificacion,
                    x.IdTipoEmpresa,
                    x.Nombre,
                    x.IdEstado

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
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar empresa " + ex.Message.ToString());
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

                var documento = identity.FindFirst("documento").Value.ToString();

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
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar empresa " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarEmpresas editarEmpresas)
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
                        title = "Editar empresa",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta tipo documento
                var tipoDocumento = await context.tiposDocumento.Where(x => x.Id.Equals(editarEmpresas.TipoDocumento)).FirstOrDefaultAsync();

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
                var tipoEmpresa = await context.tiposEmpresas.Where(x => x.Id.Equals(editarEmpresas.IdTipoEmpresa)).FirstOrDefaultAsync();

                if (tipoEmpresa == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar empresa",
                        status = 404,
                        message = "Tipo empresa no encontrado"
                    });
                }

                //Consulta de empresa del usuario
                var existe = await context.empresas.Where(x => x.Id.Equals(editarEmpresas.Id)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar empresa",
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
                        r.TipoDocumento = editarEmpresas.TipoDocumento;
                        r.Documento = editarEmpresas.Documento != null ? editarEmpresas.Documento : "";
                        r.Nombre = editarEmpresas.Nombre != null ? editarEmpresas.Nombre : "";
                        r.Descripcion = editarEmpresas.Descripcion != null ? editarEmpresas.Descripcion : "";
                        r.IdTipoEmpresa = editarEmpresas.IdTipoEmpresa;
                        r.DigitoVerificacion = editarEmpresas.DigitoVerificacion;
                        r.UsuarioModifico = usuario.Document;
                        r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                    });
                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar empresa",
                    status = 200,
                    message = "Empresa actualizada"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar empresa " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar empresa",
                    status = 400,
                    message = ""
                }); ;
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

                var documento = identity.FindFirst("documento").Value.ToString();
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
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar empresa " + ex.Message.ToString());
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
