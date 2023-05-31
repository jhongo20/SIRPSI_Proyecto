using DataAccess.Context;
using EmailServices;
using EvertecApi.Log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace SIRPSI
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsApi",
                    builder => builder.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            services.AddControllers();

            //Documentos de Api
            services.AddSwaggerGen();

            //Mapeo de base de datos en perfiles a la configuración
            services.AddAutoMapper(typeof(Startup));

            //Configuración de logs de errores
            services.AddSingleton<ILoggerManager, LoggerManager>();

            //Configuración de la base de datos
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("database")));

            //Configuración de autenticación
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option => option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["KeyJwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddEndpointsApiExplorer();

            //Configuración de envio de tokens y validació del mismo
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });
            });

            var emailConfig = Configuration
             .GetSection("EmailConfiguration")
             .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);

            services.AddScoped<IEmailSender, EmailSender>();

            services.AddIdentity<IdentityUser, IdentityRole>()
                 .AddEntityFrameworkStores<AppDbContext>()
                 .AddDefaultTokenProviders();
        }

        public void configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRouting();
            //Uso de Cors
            app.UseCors("CorsApi");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
