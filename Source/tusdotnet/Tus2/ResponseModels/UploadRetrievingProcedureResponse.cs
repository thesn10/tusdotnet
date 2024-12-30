using tusdotnet.Routing;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Storage.Results.Tus2;

namespace tusdotnet.Tus2
{
    public class UploadRetrievingProcedureResponse : Tus2BaseResponse
    {
        public bool UploadIncomplete { get; set; }

        public UploadRetrievingProcedureResponse()
        {
            NoCache = true;
            Status = HttpStatusCode.NoContent;
        }

        public UploadRetrievingProcedureResponse(RetrieveOffsetResult retrieveOffsetResult)
            : this()
        {
            UploadIncomplete = retrieveOffsetResult.UploadIncomplete;
            UploadOffset = retrieveOffsetResult.UploadOffset;
        }

        public override Task Execute(TusContext context)
        {
            context.HttpContext.SetHeader("Upload-Incomplete", UploadIncomplete.ToSfBool());

            return Task.CompletedTask;
        }
    }
}