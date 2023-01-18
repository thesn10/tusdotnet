using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AspNetCore_netcoreapp3._1_TestApp.Authentication;
using AspNetCore_netcoreapp3._1_TestApp.Endpoints;
using AspNetCore_netcoreapp3_1_TestApp.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using tusdotnet;
using tusdotnet.Helpers;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;
using tusdotnet.Tus2;

namespace AspNetCore_netcoreapp3._1_TestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        public static string DirectoryPath => Configuration.GetValue<string>("FolderDiskPath");

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            //services.AddHostedService<ExpiredFilesCleanupService>();

            services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            // Later used inside MyTusController to limit creation to this policy.
            services.AddAuthorization(opt => opt.AddPolicy("create-file-policy", builder => builder.RequireRole("create-file")));

            services.AddLogging(builder => builder.AddConsole());

            services
                .AddTus()
                .AddControllerServices(config =>
                {
                    config.AddController<MyTusController>();
                    config.AddEndpointServices();
                })
                .AddStorage("my-storage", new TusDiskStore(@"C:\tusfiles\"), isDefault: true);

            services.AddOptions();

            services.Configure<Tus2Options>(Configuration);

            services.AddTus2(options =>
            {
                options.AddStorageFactory(new SimpleTus2StorageFactory());
                options.AddDiskStorage(@"C:\tusfiles");
                //options.AddDiskBasedUploadManager(@"C:\tusfiles");
                options.AddHandler<MyTusHandler>();
                options.AddHandler<OnlyCompleteTusHandler>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logger = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger<Startup>();

            app.Use((context, next) =>
            {
                // Default limit was changed some time ago. Should work by setting MaxRequestBodySize to null using ConfigureKestrel but this does not seem to work for IISExpress.
                // Source: https://github.com/aspnet/Announcements/issues/267
                context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
                return next.Invoke();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSimpleExceptionHandler();

            app.UseAuthentication();

            //app.UseDefaultFiles();
            //app.UseStaticFiles();

            //app.UseHttpsRedirection();

            app.UseCors(builder => builder
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowAnyOrigin()
               .WithExposedHeaders(CorsHelper.GetExposedHeaders()));

            // httpContext parameter can be used to create a tus configuration based on current user, domain, host, port or whatever.
            // In this case we just return the same configuration for everyone.
            //app.UseTus(httpContext => Task.FromResult(httpContext.RequestServices.GetService<DefaultTusConfiguration>()));

            app.UseRouting();

            // All GET requests to tusdotnet are forwarded so that you can handle file downloads.
            // This is done because the file's metadata is domain specific and thus cannot be handled 
            // in a generic way by tusdotnet.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapTus2<MyTusHandler>("/files-tus-2");
                endpoints.MapTus2<OnlyCompleteTusHandler>("/files-tus-2-only-complete");
                endpoints.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute);
                endpoints.Map("/files-tus-2-info", Tus2InfoEndpoint.Invoke);
                endpoints.MapGet("/random-file-id", async httpContext =>
                {
                    var arr = ArrayPool<byte>.Shared.Rent(32);
                    var span = arr.AsMemory()[..32];
                    RandomNumberGenerator.Fill(span.Span);

                    await httpContext.Response.WriteAsync(":" + Convert.ToBase64String(span.Span) + ":");

                    ArrayPool<byte>.Shared.Return(arr, clearArray: true);
                });
            });
            app.UseAuthorization();

            app.UseEndpoints(endpoints => 
            {
                // Example of a custom download enpoint (which is domain specific
                // and thus cannot be handled in a generic way by tusdotnet)
                endpoints.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute);

                // Map a tus controller
                endpoints.MapTusController<MyTusController>("files").RequireAuthorization();

                // If you dont need to write your own controller, you can use this simpler abstraction:
                endpoints.MapTus("/other-files/{fileId?}", (options) =>
                {
                    options.StorageProfile = "my-storage";
                    options.Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(Constants.FileExpirationInMinutes));
                    options.MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues;
                    options.Events = new Events
                    {
                        OnFileCompleteAsync = ctx =>
                        {
                            logger.LogInformation($"Upload of {ctx.FileId} completed using {ctx.Store.GetType().FullName}");
                            return Task.CompletedTask;
                        }
                    };
                });
            });
        }
    }
}
