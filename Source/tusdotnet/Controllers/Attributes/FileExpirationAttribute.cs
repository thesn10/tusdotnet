using System;
using tusdotnet.Models.Expiration;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Type of expiration
    /// </summary>
    public enum ExpirationType
    {
        /// <summary>
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// </summary>
        Sliding,
        /// <summary>
        /// Absolute expiration will be saved per file when the file is created.
        /// </summary>
        Absolute,
    }

    /// <summary>
    /// Set an expiration time where incomplete files can no longer be updated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FileExpirationAttribute : Attribute
    {
        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// </summary>
        public FileExpirationAttribute(int timeInMinutes, ExpirationType expirationType)
        {
            if (expirationType == ExpirationType.Sliding)
            {
                Expiration = new SlidingExpiration(TimeSpan.FromMinutes(timeInMinutes));
            }
            else if (expirationType == ExpirationType.Absolute)
            {
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(timeInMinutes));
            }
        }

        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// This value can either be <c>AbsoluteExpiration</c> or <c>SlidingExpiration</c>.
        /// Absolute expiration will be saved per file when the file is created.
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// </summary>
        public ExpirationBase Expiration { get; }
    }
}
