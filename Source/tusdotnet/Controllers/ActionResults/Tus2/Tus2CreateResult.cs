using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Routing;
using tusdotnet.Tus2;

namespace tusdotnet.Controllers.ActionResults
{
    public class Tus2CreateResult : ITus2CreateResult
    {
        public Tus2CreateResult(long? uploadOffset, long? uploadComplete, string? location)
        {
            Location = location;
            UploadOffset = uploadOffset;
            UploadComplete = uploadComplete;
        }


        /// <inheritdoc />
        public bool IsSuccessResult => true;

        public string? Location { get; set; }
        public long? UploadOffset { get; set; }
        public long? UploadComplete { get; set; }

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            var statusCode = UploadOffset is not null ? 
                HttpStatusCode.Created : 
                (HttpStatusCode)104; // (Upload Resumption Supported)
            
            var result = new Tus2BaseResult(statusCode)
            {
                NoCache = true,
                UploadOffset = UploadOffset,
                UploadComplete = UploadComplete,
            };
            
            if (Location != null)
            {
                context.HttpContext.SetHeader("Location", Location.ToString());
            }

            return result.Execute(context);
        }
    }
}
