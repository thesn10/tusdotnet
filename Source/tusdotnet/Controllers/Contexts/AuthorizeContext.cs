using System;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Models;
using tusdotnet.RequestHandlers;
using tusdotnet.Routing;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Context for the Authorize action
    /// </summary>
    public class AuthorizeContext
    {
        /// <summary>
        /// The intent of the request, i.e. what action the request is trying to perform
        /// </summary>
        public IntentType IntentType { get; set; }

        /// <summary>
        /// The file id of the request (if applicable)
        /// </summary>
        public string? FileId { get; set; }

        internal RequestHandler RequestHandler { get; set; }
        internal TusControllerBase Controller { get; set; }

        /// <summary>
        /// Gets the method to authorize. Useful for reading method attributes
        /// </summary>
        public MethodInfo GetMethodToAuthorize()
        {
#if NETCOREAPP2_0_OR_GREATER
            return IntentType switch
            {
                IntentType.WriteFile => ((Func<WriteContext, Task<IWriteResult>>)Controller.Write).Method,
                IntentType.CreateFile => ((Func<CreateContext, Task<ICreateResult>>)Controller.Create).Method,
                IntentType.ConcatenateFiles => ((Func<CreateContext, Task<ICreateResult>>)Controller.Create).Method,
                IntentType.DeleteFile => ((Func<DeleteContext, Task<ISimpleResult>>)Controller.Delete).Method,
                IntentType.GetFileInfo => ((Func<GetFileInfoContext, Task<IFileInfoResult>>)Controller.GetFileInfo).Method,
                IntentType.GetOptions => ((Func<Task<FeatureSupportContext>>)Controller.GetOptions).Method,
                _ => throw new ArgumentException(),
            };
#else
            return IntentType switch
            {
                IntentType.WriteFile => Controller.GetType().GetMethod(nameof(Controller.Write)),
                IntentType.CreateFile => Controller.GetType().GetMethod(nameof(Controller.Create)),
                IntentType.ConcatenateFiles => Controller.GetType().GetMethod(nameof(Controller.Create)),
                IntentType.DeleteFile => Controller.GetType().GetMethod(nameof(Controller.Delete)),
                IntentType.GetFileInfo => Controller.GetType().GetMethod(nameof(Controller.GetFileInfo)),
                IntentType.GetOptions => Controller.GetType().GetMethod(nameof(Controller.GetOptions)),
                _ => throw new ArgumentException(),
            };
#endif
        }
    }
}