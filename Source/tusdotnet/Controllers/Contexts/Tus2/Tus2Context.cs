using Microsoft.AspNetCore.Http;
using System.Threading;
using tusdotnet.Tus2;

namespace tusdotnet.Controllers.Contexts.Tus2
{
    public abstract class Tus2Context
    {
        internal HttpContext HttpContext { get; set; }

        public Tus2Headers Headers { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}
