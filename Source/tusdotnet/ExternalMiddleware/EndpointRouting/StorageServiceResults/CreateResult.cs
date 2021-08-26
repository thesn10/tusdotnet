using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class CreateResult
    {
        public string FileId { get; set; }
        public DateTimeOffset? FileExpires { get; internal set; }
    }
}
