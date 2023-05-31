using SIRPSI;

var builder = WebApplication.CreateBuilder(args);

//Configuraci�n de la clase startup
var startup = new Startup(builder.Configuration);

//Configuraci�n de servicios
startup.ConfigureServices(builder.Services);
var app = builder.Build();

//Configuraci�n de variables
startup.configure(app, app.Environment);

app.Run();