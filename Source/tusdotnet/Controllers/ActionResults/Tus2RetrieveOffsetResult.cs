using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Routing;

namespace tusdotnet.Controllers.ActionResults
{
    public class Tus2RetrieveOffsetResult : ITus2RetrieveOffsetResult
    {
        public Tus2RetrieveOffsetResult(long uploadOffset, bool uploadIncomplete)
        {
            UploadOffset = uploadOffset;
            UploadIncomplete = uploadIncomplete;
        }


        /// <inheritdoc />
        public bool IsSuccessResult => true;

        public long UploadOffset { get; set; }
        public bool UploadIncomplete { get; set; }

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            var result = new Tus2BaseResult(HttpStatusCode.NoContent)
            {
                NoCache = true,
                UploadOffset = UploadOffset,
                UploadIncomplete = UploadIncomplete,
            };

            return result.Execute(context);
        }
    }
}
