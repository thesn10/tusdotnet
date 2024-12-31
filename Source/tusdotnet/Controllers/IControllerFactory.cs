using Microsoft.AspNetCore.Http;

namespace tusdotnet.Controllers
{
    internal interface IControllerFactory
    {
        TusControllerBase? CreateController(HttpContext httpContext);

        Tus2ControllerBase? CreateV2Controller(HttpContext httpContext);
    }
}
