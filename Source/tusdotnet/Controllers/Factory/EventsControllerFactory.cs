using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting;

namespace tusdotnet.Controllers.Factory
{
    internal class EventsControllerFactory : IControllerFactory
    {
        private readonly TusSimpleEndpointOptions _options;

        public EventsControllerFactory(TusSimpleEndpointOptions options) 
        {
            _options = options;
        }

        public TusControllerBase CreateController(HttpContext httpContext)
        {
            return new EventsBasedTusController(_options);
        }

        public Tus2ControllerBase CreateV2Controller(HttpContext httpContext)
        {
            return null;
        }
    }
}
