using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Estados;
using DataAccess.Models.WorkPlace;
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
using SIRPSI.DTOs.WorkPlace;
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.WorkPlace
{
    [Route("api/centrotrabajo")]
    [ApiController]
    [EnableCors("CorsApi")]
    public class CentroTrabajoController : ControllerBase
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
        public CentroTrabajoController(AppDbContext context,
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
        [HttpGet("ConsultarCentroDeTrabajo", Name = "consultarCentroDeTrabajo")]
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
                        title = "Consultar centro de trabajo",
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
                        title = "Consultar centro de trabajo",
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
                    var centrosDeTrabajo = await context.centroTrabajo.Select(x => new
                    {
                        x.Id,
                        x.Nombre,
                        x.Descripcion
                    }).ToListAsync();

                    if (centrosDeTrabajo == null)
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        return NotFound(new General()
                        {
                            title = "Consultar centro de trabajo",
                            status = 404,
                            message = "Centros de trabajo no encontrados"
                        });
                    }

                    //Retorno de los datos encontrados
                    return centrosDeTrabajo;
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Consultar centro de trabajo",
                        status = 400,
                        message = "No tiene permisos para consultar centros de trabajo"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar centro de trabajo " + ex.Message.ToString() + " - " + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Consultar centro de trabajo",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Registro
        [HttpPost("RegistrarCentroDeTrabajo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarCentroTrabajo registrarCentroTrabajo)
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
                        title = "Registrar centro de trabajo",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Si existe empresa
                var empresa = context.empresas.Where(u => u.Id.Equals(registrarCentroTrabajo.IdEmpresa)).FirstOrDefault();

                if (empresa == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar centro de trabajo",
                        status = 404,
                        message = "Empresa no encontrada"
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
                        title = "Registrar centro de trabajo",
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
                    var centroTrabajo = mapper.Map<CentroTrabajo>(registrarCentroTrabajo);
                    //Valores asignados
                    centroTrabajo.Id = Guid.NewGuid().ToString();
                    centroTrabajo.Nombre = registrarCentroTrabajo.Nombre != null ? registrarCentroTrabajo.Nombre : "";
                    centroTrabajo.Descripcion = registrarCentroTrabajo.Descripcion;
                    centroTrabajo.IdEmpresa = registrarCentroTrabajo.IdEmpresa;
                    centroTrabajo.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(1)).Select(x => x.Id).First();
                    centroTrabajo.UsuarioRegistro = usuario.Document != null ? usuario.Document : "";
                    centroTrabajo.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                    centroTrabajo.UsuarioModifico = null;
                    centroTrabajo.UsuarioModifico = null;

                    //Agregar datos al contexto
                    context.Add(centroTrabajo);
                    //Guardado de datos 
                    await context.SaveChangesAsync();

                    return Created("", new General()
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Registrar centro de trabajo",
                        status = 201,
                        message = "Centro de trabajo creado"
                    });
                }
                else
                {
                    return BadRequest(new General()
                    {
                        title = "Registrar centro de trabajo",
                        status = 400,
                        message = "No tiene permisos para registrar centros de trabajo"
                    });
                }
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar centro de trabajo " + ex.Message.ToString() + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "Registrar centro de trabajo",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

    }
}
