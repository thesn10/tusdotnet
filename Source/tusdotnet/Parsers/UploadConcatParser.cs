using tusdotnet.Parsers.UploadConcatParserHelpers;
using tusdotnet.Routing;

namespace tusdotnet.Parsers
{
    internal static class UploadConcatParser
    {
        internal static UploadConcatParserResult ParseAndValidate(string uploadConcatHeader, ITusRoutingHelper routingHelper)
        {
#if NETCOREAPP3_1_OR_GREATER
    
            return UploadConcatParserSpanBased.ParseAndValidate(uploadConcatHeader, routingHelper);
#else
            return UploadConcatParserStringBased.ParseAndValidate(uploadConcatHeader, routingHelper);

#endif
        }
	}
}
