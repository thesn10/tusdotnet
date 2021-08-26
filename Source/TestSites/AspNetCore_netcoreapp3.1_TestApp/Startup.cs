using AspNetCore_netcoreapp3._1_TestApp.Authentication;
using AspNetCore_netcoreapp3._1_TestApp.Endpoints;
using AspNetCore_netcoreapp3_1_TestApp.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Helpers;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace AspNetCore_netcoreapp3._1_TestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            // Only needed for output formatting.
            // TODO: Might not want to rely on Mvc being added? Could just include our own plain text formatter if AddMvc has not been added.
            services.AddMvc();

            services.AddLogging(builder => builder.AddConsole());

            services.AddTus();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseCors(builder => builder
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowAnyOrigin()
               .WithExposedHeaders(CorsHelper.GetExposedHeaders()));

            // httpContext parameter can be used to create a tus configuration based on current user, domain, host, port or whatever.
            // In this case we just return the same configuration for everyone.
            //app.UseTus(httpContext => Task.FromResult(httpContext.RequestServices.GetService<DefaultTusConfiguration>()));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => 
            {
                // Example of a custom download enpoint (which is domain specific
                // and thus cannot be handled in a generic way by tusdotnet)
                endpoints.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute);

                endpoints.MapTus(tusEndpoints =>
                {
                    tusEndpoints.MapController<MyTusController>("/files").RequireAuthorization();

                    // If you dont want to write your own controller, you can use this abstraction:
                    tusEndpoints.Map("/simple/files", (options, storageOptions) =>
                    {
                        options.MetadataParsingStrategy = MetadataParsingStrategy.Original;

                        storageOptions.UseDiskStore(Constants.FileDirectory);
                        storageOptions.Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(Constants.FileExpirationInMinutes));
                    });
                });
            });
        }
    }
}
