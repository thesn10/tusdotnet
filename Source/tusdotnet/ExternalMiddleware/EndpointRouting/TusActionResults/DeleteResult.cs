#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface IDeleteResult
    {
    }

    public class TusDeleteOk : IDeleteResult
    {
        public TusDeleteOk()
        {
        }
    }
}

#endif