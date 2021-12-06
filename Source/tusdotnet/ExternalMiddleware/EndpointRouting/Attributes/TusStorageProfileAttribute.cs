using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Specifies the storage profile that the controller should use
    /// </summary>
    public class TusStorageProfileAttribute : Attribute
    {
        /// <summary>
        /// Specifies the storage profile that the controller should use
        /// </summary>
        public TusStorageProfileAttribute(string profileName)
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// Name of the storage profile
        /// </summary>
        public string ProfileName { get; }
    }
}
