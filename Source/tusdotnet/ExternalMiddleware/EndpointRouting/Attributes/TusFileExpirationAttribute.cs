using System;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileExpirationAttribute : Attribute
    {
        public TusFileExpirationAttribute(int timeInMinutes, bool sliding = false)
        {
            if (sliding)
            {
                Expiration = new SlidingExpiration(TimeSpan.FromMinutes(timeInMinutes));
            }
            else
            {
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(timeInMinutes));
            }
        }

        public ExpirationBase Expiration { get; }
    }
}
