using System;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
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
        /// The controller method to be authorized
        /// </summary>
        public MethodInfo ControllerMethod { get; set; }

        /// <summary>
        /// The file id of the request (if applicable)
        /// </summary>
        public string? FileId { get; set; }

        internal RequestHandler RequestHandler { get; set; }

        internal static MethodInfo GetControllerActionMethodInfo(IntentType intent, TusControllerBase controller)
        {
#if NET20_OR_GREATER
            return intent switch
            {
                IntentType.WriteFile => ((Func<WriteContext, Task<IWriteResult>>)controller.Write).Method,
                IntentType.CreateFile => ((Func<CreateContext, Task<ICreateResult>>)controller.Create).Method,
                IntentType.ConcatenateFiles => ((Func<CreateContext, Task<ICreateResult>>)controller.Create).Method,
                IntentType.DeleteFile => ((Func<DeleteContext, Task<ISimpleResult>>)controller.Delete).Method,
                IntentType.GetFileInfo => ((Func<GetFileInfoContext, Task<IFileInfoResult>>)controller.GetFileInfo).Method,
                IntentType.GetOptions => ((Func<Task<TusExtensionInfo>>)controller.GetOptions).Method,
                _ => throw new ArgumentException(),
            };
#else
            return intent switch
            {
                IntentType.WriteFile => controller.GetType().GetMethod(nameof(controller.Write)),
                IntentType.CreateFile => controller.GetType().GetMethod(nameof(controller.Create)),
                IntentType.ConcatenateFiles => controller.GetType().GetMethod(nameof(controller.Create)),
                IntentType.DeleteFile => controller.GetType().GetMethod(nameof(controller.Delete)),
                IntentType.GetFileInfo => controller.GetType().GetMethod(nameof(controller.GetFileInfo)),
                IntentType.GetOptions => controller.GetType().GetMethod(nameof(controller.GetOptions)),
                _ => throw new ArgumentException(),
            };
#endif
        }
    }
}