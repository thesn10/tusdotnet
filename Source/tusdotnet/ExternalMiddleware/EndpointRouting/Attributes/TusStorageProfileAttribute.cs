using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusStorageProfileAttribute : Attribute
    {
        public TusStorageProfileAttribute(string profileName)
        {
            ProfileName = profileName;
        }

        public string ProfileName { get; }
    }
}
