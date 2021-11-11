using Microsoft.Extensions.Primitives;
using System.Net;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;

namespace AspNetCore_net6._0_TestApp
{
    /// <summary>
    /// Alternative way of confinguring tus instead of a controller
    /// </summary>
    public class MyTusEndpoint
    {
        public static void ConfigureTus(TusSimpleEndpointOptions opts, WebApplication app)
        {
            // Change the value of EnableOnAuthorize in appsettings.json to enable or disable
            // the new authorization event.
            var enableAuthorize = app.Configuration.Get<OnAuthorizeOption>().EnableOnAuthorize;

            opts.StorageProfile = "my-storage";
            opts.MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues;
            opts.UsePipelinesIfAvailable = true;
            opts.Events = new Events
            {
                OnAuthorizeAsync = ctx =>
                {
                    if (!enableAuthorize)
                        return Task.CompletedTask;

                    if (ctx.HttpContext.User.Identity?.IsAuthenticated != true)
                    {
                        ctx.HttpContext.Response.Headers.Add("WWW-Authenticate", new StringValues("Basic realm=tusdotnet-test-net6.0"));
                        ctx.FailRequest(HttpStatusCode.Unauthorized);
                        return Task.CompletedTask;
                    }

                    if (ctx.HttpContext.User.Identity.Name != "test")
                    {
                        ctx.FailRequest(HttpStatusCode.Forbidden, "'test' is the only allowed user");
                        return Task.CompletedTask;
                    }

                    // Do other verification on the user; claims, roles, etc.

                    // Verify different things depending on the intent of the request.
                    // E.g.:
                    //   Does the file about to be written belong to this user?
                    //   Is the current user allowed to create new files or have they reached their quota?
                    //   etc etc
                    switch (ctx.Intent)
                    {
                        case IntentType.CreateFile:
                            break;
                        case IntentType.ConcatenateFiles:
                            break;
                        case IntentType.WriteFile:
                            break;
                        case IntentType.DeleteFile:
                            break;
                        case IntentType.GetFileInfo:
                            break;
                        case IntentType.GetOptions:
                            break;
                        default:
                            break;
                    }

                    return Task.CompletedTask;
                },

                OnBeforeCreateAsync = ctx =>
                {
                    // Partial files are not complete so we do not need to validate
                    // the metadata in our example.
                    if (ctx.FileConcatenation is FileConcatPartial)
                    {
                        return Task.CompletedTask;
                    }

                    if (!ctx.Metadata.ContainsKey("name") || ctx.Metadata["name"].HasEmptyValue)
                    {
                        ctx.FailRequest("name metadata must be specified. ");
                    }

                    if (!ctx.Metadata.ContainsKey("contentType") || ctx.Metadata["contentType"].HasEmptyValue)
                    {
                        ctx.FailRequest("contentType metadata must be specified. ");
                    }

                    return Task.CompletedTask;
                },
                OnCreateCompleteAsync = ctx =>
                {
                    app.Logger.LogInformation($"Created file {ctx.FileId} using {ctx.Store.GetType().FullName}");
                    return Task.CompletedTask;
                },
                OnBeforeDeleteAsync = ctx =>
                {
                    // Can the file be deleted? If not call ctx.FailRequest(<message>);
                    return Task.CompletedTask;
                },
                OnDeleteCompleteAsync = ctx =>
                {
                    app.Logger.LogInformation($"Deleted file {ctx.FileId} using {ctx.Store.GetType().FullName}");
                    return Task.CompletedTask;
                },
                OnFileCompleteAsync = ctx =>
                {
                    app.Logger.LogInformation($"Upload of {ctx.FileId} completed using {ctx.Store.GetType().FullName}");
                    // If the store implements ITusReadableStore one could access the completed file here.
                    // The default TusDiskStore implements this interface:
                    //var file = await ctx.GetFileAsync();
                    return Task.CompletedTask;
                }
            };

            // Set an expiration time where incomplete files can no longer be updated.
            // This value can either be absolute or sliding.
            // Absolute expiration will be saved per file on create
            // Sliding expiration will be saved per file on create and updated on each patch/update.
            opts.Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5));
        }
    }
}
