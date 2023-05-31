﻿using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Companies;
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
using SIRPSI.Helpers.Answers;
using System.Security.Claims;

namespace SIRPSI.Controllers.Companies
{
    [Route("api/tiposempresa")]
    [ApiController]
    public class TiposEmpresaController : ControllerBase
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
        public TiposEmpresaController(AppDbContext context,
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
        [HttpGet("ConsultarTipoEmpresa", Name = "consultarTiposEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> Get([FromBody] ConsultarTipoEmpresa consultarTipoEmpresa)
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
                        title = "Registrar tipo empresas",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }
                //Consultar estados
                var estado = await context.estados.Where(x => x.Id.Equals(consultarTipoEmpresa.IdEstado)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Estados tipo empresa",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Consulta el tipo empresa
                var tipoEmpresa = context.tiposEmpresas.Where(x => x.IdEstado.Equals(estado.Id)).Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Descripcion,
                    x.IdEstado

                }).ToList();

                if (tipoEmpresa == null)
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    return NotFound(new General()
                    {
                        title = "Consultar tipo empresa",
                        status = 404,
                        message = "Tipo empresa no encontrada"
                    });
                }

                //Retorno de los datos encontrados
                return tipoEmpresa;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Consultar tipo empresa" + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Consultar tipo empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }

        #endregion

        #region Registro
        [HttpPost("RegistrarTipoEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RegistrarTipoEmpresa registrarTipoEmpresa)
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
                        title = "Registrar tipo empresa",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                var estado = await context.estados.Where(x => x.Id.Equals(registrarTipoEmpresa.IdEstado)).FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound(new General()
                    {
                        title = "Registrar tipo empresa",
                        status = 404,
                        message = "Estado no encontrado"
                    });
                }
                //Mapeo de datos en clases
                var tipoEmpresa = mapper.Map<TiposEmpresa>(registrarTipoEmpresa);
                //Valores asignados
                tipoEmpresa.Id = Guid.NewGuid().ToString();
                tipoEmpresa.Nombre = registrarTipoEmpresa.Nombre;
                tipoEmpresa.Descripcion = registrarTipoEmpresa.Descripcion;
                tipoEmpresa.IdEstado = estado.Id;
                tipoEmpresa.UsuarioRegistro = usuario.Document;
                tipoEmpresa.FechaRegistro = DateTime.Now.ToDateTimeZone().DateTime;
                tipoEmpresa.FechaModifico = null;
                tipoEmpresa.UsuarioModifico = null;

                //Agregar datos al contexto
                context.Add(tipoEmpresa);
                //Guardado de datos 
                await context.SaveChangesAsync();

                return Created("", new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Registrar tipo empresa",
                    status = 201,
                    message = "Tipo empresa creada"
                }); ;
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Registrar tipo empresa " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Registrar tipo empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region Editar
        [HttpPut("EditarTipoEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(EditarTipoEmpresa editarTipoEmpresa)
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
                        title = "Editar tipo empresa",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consulta de empresa del usuario
                var existe = await context.tiposEmpresas.Where(x => x.Id.Equals(editarTipoEmpresa.Id)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    return NotFound(new
                    {
                        //Visualizacion de mensajes al usuario del aplicativo
                        title = "Editar tipo empresa",
                        status = 404,
                        message = "Tipo empresa no encontrada"
                    });
                }
               
                //Registro de datos
                context.tiposEmpresas.Where(x => x.Id.Equals(existe.Id)).ToList()
                    .ForEach(r =>
                    {
                        r.Nombre = editarTipoEmpresa.Nombre;
                        r.Descripcion = editarTipoEmpresa.Descripcion;                       
                        r.UsuarioModifico = usuario.Document;
                        r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                    });
                //Guardado de datos
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "Editar tipo empresa",
                    status = 200,
                    message = "Tipo empresa actualizada"
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Editar tipo empresa " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Editar tipo empresa",
                    status = 400,
                    message = ""
                }); ;
            }
        }
        #endregion

        #region Eliminar
        [HttpDelete("EliminarTipoEmpresa")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] EliminarTipoEmpresa eliminarTipoEmpresa)
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
                        title = "Eliminar tipo empresa",
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
                        title = "Eliminar tipo empresa",
                        status = 404,
                        message = "Estados no encontrados"
                    });
                }

                //Consulta de empresa
                var existe = await context.tiposEmpresas.Where(x => x.Id.Equals(eliminarTipoEmpresa.Id)).FirstOrDefaultAsync();

                if (existe == null)
                {
                    return NotFound(new General()
                    {
                        title = "Eliminar tipo empresa",
                        status = 404,
                        message = "Tipo empresa no encontrada"
                    });
                }

                //Agregar datos al contexto
                context.tiposEmpresas.Where(x => x.Id.Equals(eliminarTipoEmpresa.Id)).ToList()
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
                    title = "Eliminar tipo empresa",
                    status = 200,
                    message = "Tipo empresa eliminada"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("Eliminar tipo empresa " + ex.Message.ToString());
                return BadRequest(new General()
                {
                    title = "Eliminar tipo empresa",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion
    }
}
