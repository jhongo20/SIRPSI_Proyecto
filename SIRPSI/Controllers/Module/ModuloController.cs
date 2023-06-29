using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SIRPSI.Controllers.Module
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsApi")]
    public class ModuloController : ControllerBase
    {
    }
}
