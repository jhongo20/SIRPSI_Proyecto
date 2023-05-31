using AutoMapper;
using DataAccess.Context;
using DataAccess.Models.Users;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using SIRPSI.Core.Helper;
using SIRPSI.DTOs.User;
using SIRPSI.DTOs.User.Roles;
using SIRPSI.Helpers.Answers;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace SIRPSI.Controllers.User
{
    [Route("api/user")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        #region Dependences
        private readonly UserManager<IdentityUser> userManager;
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        //Constructor 
        public UserAccountsController(AppDbContext context,
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

        #region RegisterUser
        [HttpPost("RegisterUser")]
        public async Task<ActionResult<AuthenticationResponse>> Register(UserCredentials userCredentials)
        {
            try
            {
                #region Validaciones

                var existTypeDoc = await context.tiposDocumento.Where(x => x.Id.Equals(userCredentials.IdTypeDocument)).FirstOrDefaultAsync();

                if (existTypeDoc == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Tipo de documento NO existe."
                    });
                }

                var existRoles = await context.AspNetRoles.Where(x => x.Id.Equals(userCredentials.IdRol)).FirstOrDefaultAsync();

                if (existRoles == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Estado del usuario NO existe."
                    });
                }

                var existCountry = await context.pais.Where(x => x.Id.Equals(userCredentials.IdCountry)).FirstOrDefaultAsync();

                if (existCountry == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Pais NO existe."
                    });
                }

                var existCompany = await context.empresas.Where(x => x.Id.Equals(userCredentials.IdCompany)).FirstOrDefaultAsync();

                if (existCompany == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Empresa NO existe."
                    });
                }

                #endregion

                #region agregar datos al contexto

                var userNet = mapper.Map<AspNetUsers>(userCredentials);

                userNet.Document = userCredentials.Document;
                userNet.IdCompany = userCredentials.IdCompany;
                userNet.IdCountry = userCredentials.IdCountry;
                userNet.TypeDocument = userCredentials.IdTypeDocument;


                if (string.IsNullOrEmpty(userNet.Email))
                {
                    userNet.UserName = userCredentials.Document + "@sirpsi.com";
                    userNet.Email = userCredentials.Document + "@sirpsi.com";
                }
                else
                {
                    userNet.UserName = userCredentials.Email;
                    userNet.Email = userCredentials.Email;
                }

                userNet.Surnames = userCredentials.Surnames;
                userNet.Names = userCredentials.Names;
                userNet.PhoneNumber = userCredentials.PhoneNumber;
                userNet.UserRegistration = userCredentials.Document;
                userNet.RegistrationDate = DateTime.Now.ToDateTimeZone().DateTime;
                userNet.UserModify = null;
                userNet.ModifiedDate = null;
                userNet.Status = userCredentials.IdEstado;

                context.Add(userNet);

                #endregion

                #region Registro de datos

                var result = await userManager.CreateAsync(userNet, userCredentials.Password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Registro de usuario ¡Exitoso!");
                    return BuildToken(userCredentials);
                }
                else
                {
                    logger.LogError("Registro de usuario ¡fallido!");

                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Registro de usuario ¡fallido!"
                    });
                }

                #endregion

            }
            catch (Exception ex)
            {
                logger.LogError("usuario " + ex.Message + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Registro de usuario ¡fallido!"
                });
            }

        }

        #endregion

        #region DeleteUser
        [HttpDelete("DeleteUser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] DeleteUser deleteUser)
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
                        title = "Usuario",
                        status = 404,
                        message = "Usuario no encontrado"
                    });
                }

                //Consultar estados
                var estados = await context.estados.ToListAsync();

                if (estados == null)
                {
                    return NotFound(new General()
                    {
                        title = "Usuario",
                        status = 404,
                        message = "Estados no encontrados"
                    });
                }

                //Agregar datos al contexto
                context.AspNetUsers.Where(x => x.Id.Equals(deleteUser.Id)).ToList()
                  .ForEach(r =>
                  {
                      r.Status = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
                      r.UserModify = usuario.Document;
                      r.ModifiedDate = DateTime.Now.ToDateTimeZone().DateTime;
                  });

                context.AspNetUserRoles.Where(x => x.UserId.Equals(deleteUser.Id)).ToList()
                .ForEach(r =>
                {
                    r.IdEstado = estados.Where(x => x.IdConsecutivo.Equals(2)).Select(x => x.Id).First();
                    r.UsuarioModifico = usuario.Document;
                    r.FechaModifico = DateTime.Now.ToDateTimeZone().DateTime;
                });

                //Se elimina el regitro
                await context.SaveChangesAsync();

                return Ok(new General()
                {
                    //Visualizacion de mensajes al usuario del aplicativo
                    title = "usuario",
                    status = 200,
                    message = "Usuario eliminado"
                });
            }
            catch (Exception ex)
            {
                //Registro de errores
                logger.LogError("usuario " + ex.Message + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }

        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<ActionResult<AuthenticationResponse>> Login(UserCredentials userCredentials)
        {
            try
            {
                var existTypeDoc = await context.tiposDocumento.Where(x => x.Id.Equals(userCredentials.IdTypeDocument)).FirstOrDefaultAsync();

                if (existTypeDoc == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Tipo de documento NO existe."
                    });
                }

                var existUser = await context.AspNetUsers.Where(x => x.Document.Equals(userCredentials.Document)).FirstOrDefaultAsync();

                if (existUser == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Usuario NO existe."
                    });
                }

                var email = existUser.Email.Trim() != null ? existUser.Email.Trim() : "";


                var result = await signInManager.PasswordSignInAsync(email,
                userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

                //var userId = userManager.FindByIdAsync();

                if (result.Succeeded)
                {
                    logger.LogInformation("Login de usuario ¡exitoso!");
                    return BuildToken(userCredentials);
                }
                else
                {
                    logger.LogError("Login de usuario ¡fallido!");
                    return BadRequest(new General()
                    {
                        title = "User",
                        status = 400,
                        message = "Login de usuario ¡fallido!"
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("usuario " + ex.Message + ex.StackTrace);

                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Login de usuario ¡fallido!"
                });
            }
        }

        #endregion

        #region RenewToken
        [HttpGet("RenewToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<AuthenticationResponse> RenewToken()
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                }

                var userDocument = identity.FindFirst("documento").Value.ToString();

                var userCredentials = new UserCredentials()
                {
                    Document = userDocument
                };

                return BuildToken(userCredentials);
            }
            catch (Exception ex)
            {
                logger.LogError("usuario " + ex.Message + ex.StackTrace);
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema"
                });
            }
        }
        #endregion

        #region SendEmail Password
        [HttpPost("SendEmailChangedPasswword")]
        public async Task<ActionResult<General>> SendEmail([FromBody] ChangedPassword changedPassword)
        {

            try
            {
                var user = context.AspNetUsers.Where(u => u.Document.Equals(changedPassword.Document)).FirstOrDefault();//Si el usuario existe

                if (user == null)
                {
                    return BadRequest(new General()
                    {
                        title = "usuario",
                        status = 400,
                        message = "Usuario no encontrado"
                    });
                }

                string code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = HttpUtility.UrlEncode(code);

                var Url = configuration["UrlService"];

                var urlCompleted = string.Format(Url + "/account/ResetPassword?userId={0}&code={1}", user.Id, code);

                var message = new Message(new string[] { user.Email }, "Cambio de contraseña. ", "cambia tu contraseña,  ingresando al siguiente link: <br/><br/>" + urlCompleted + "<br/> <br/>Cordialmente: <br/> <br/>" + "Sirpsi");
                await emailSender.SendEmailAsync(message);

                return Ok(new General()
                {
                    title = "usuario",
                    status = 200,
                    message = "Se ha enviado un link a tu correo electrónico, para reinicar tu contraseña."
                });
            }
            catch (Exception ex)
            {
                logger.LogError("usuario " + ex.Message + ex.StackTrace);

                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Usuario no encontrado"
                });
            }


        }
        #endregion

        #region Token
        private AuthenticationResponse BuildToken(UserCredentials userCredentials)
        {
            //var user = context.Users.Where(u => u.Email == userCredentials.Document).FirstOrDefault();

            var user = context.AspNetUsers.Where(x => x.Document.Equals(userCredentials.Document)).FirstOrDefault();

            var roles =  context.AspNetUserRoles.Where(x => x.UserId.Equals(user.Id)).Select(x => x.RoleId).ToList();

            var rolesConcatenados = "";

            if (roles != null || roles.Count != 0) 
            {
                rolesConcatenados = string.Join(", ", roles);
            }

            var email = userCredentials.Email;

            var claims = new List<Claim>()
            {
                new Claim("documento", user.Document != null ?user.Document : ""),
                new Claim("email", user.Email != null ? user.Email: ""),
                new Claim("roles", rolesConcatenados != null ? rolesConcatenados : ""),
                new Claim("estado", user.Status != null ? user.Status : ""),
                new Claim("empresa", user.IdCompany != null ? user.IdCompany : "")

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["KeyJwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(5);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: creds);
            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiration,
            };
        }
        #endregion

        #region Recuperar constraseña

        [HttpPost("RecoverPassword")]
        public async Task<ActionResult<General>> RecoverPassword(RecoverPassword recoverPassword)
        {
            if (recoverPassword.NewPassword != recoverPassword.ConfirmPassword)
            {
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Contraseñas ingresadas no coinciden"
                });
            }

            var user = context.AspNetUsers.Where(u => u.Document.Equals(recoverPassword.Document)).FirstOrDefault();//Si el usuario existe

            if (user == null)
            {
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Usuario no encontrado"
                });
            }

            var actualUser = await userManager.FindByNameAsync(user.UserName);

            bool isCorrectPwd = false;

            if (actualUser != null)
            {
                isCorrectPwd = await userManager.CheckPasswordAsync(actualUser, recoverPassword.OldPassword);
            }

            if (isCorrectPwd.Equals(false))
            {
                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Contraseña anterior no es correcta"
                });
            }

            var token = userManager.GeneratePasswordResetTokenAsync(user);
            var resultado = await userManager.ResetPasswordAsync(user, token.Result, recoverPassword.NewPassword);

            if (resultado.Succeeded)
            {
                return Ok(new General()
                {
                    title = "usuario",
                    status = 200,
                    message = "Cambio de contraseña, ¡exitoso!"

                });
            }
            else
            {
                logger.LogError(resultado.Errors.Select(x => x.Description).First());

                return BadRequest(new General()
                {
                    title = "usuario",
                    status = 400,
                    message = "Contacte con el administrador del sistema."
                });
            }
        }
        #endregion

    }
}
