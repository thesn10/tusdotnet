using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.Tus2
{
    public class UploadCreationProcedureResponse : Tus2BaseResponse
    {
        public string Location { get; set; }
        public long? UploadOffset { get; set; }
        public long? UploadComplete { get; set; }

        public UploadCreationProcedureResponse()
        {
            Status = HttpStatusCode.Created;
        }

        public override async Task Execute(TusContext context)
        {
            if (UploadOffset != null)
            {
                context.HttpContext.SetHeader("Upload-Offset", UploadOffset.ToString());
            }

            if (UploadComplete != null)
            {
                context.HttpContext.SetHeader("Upload-Complete", UploadComplete.ToString());
            }

            if (Location != null)
            {
                context.HttpContext.SetHeader("Location", Location.ToString());
            }

            if (UploadOffset is null)
            {
                Status = (HttpStatusCode)104; // (Upload Resumption Supported)
            }


            await base.Execute(context);
        }
    }
}
