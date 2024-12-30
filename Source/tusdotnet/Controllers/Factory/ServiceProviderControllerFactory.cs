using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tusdotnet.Controllers.Factory
{
    internal class ServiceProviderControllerFactory<TV1, TV2> : IControllerFactory
        where TV1 : TusControllerBase
        where TV2 : Tus2ControllerBase
    {
        public TusControllerBase CreateController(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<TV1>();
        }

        public Tus2ControllerBase CreateV2Controller(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<TV2>();
        }
    }
}
