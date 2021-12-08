using System;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Specifies the storage profile that the controller should use
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StorageProfileAttribute : Attribute
    {
        /// <summary>
        /// Specifies the storage profile that the controller should use
        /// </summary>
        public StorageProfileAttribute(string profileName)
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// Name of the storage profile
        /// </summary>
        public string ProfileName { get; }
    }
}
