using AspNetCore_net6._0_TestApp;
using AspNetCore_net6._0_TestApp.Authentication;
using AspNetCore_net6._0_TestApp.Endpoints;
using AspNetCore_net6._0_TestApp.Services;
using Microsoft.AspNetCore.Authentication;
using tusdotnet;
using tusdotnet.Controllers;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

AddTus(builder);
AddAuthorization(builder);

builder.Services.AddHostedService<ExpiredFilesCleanupService>();

var app = builder.Build();

app.UseAuthentication();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute);

app.MapTusControllerRoute<MyTusController>("/files/{fileId?}");
// or
//app.MapTus("/files", options => MyTusEndpoint.ConfigureTus(options, app));

app.Run();

static void AddAuthorization(WebApplicationBuilder builder)
{
    builder.Services.Configure<OnAuthorizeOption>(opt => opt.EnableOnAuthorize = (bool)builder.Configuration.GetValue(typeof(bool), "EnableOnAuthorize"));
    builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
}

static void AddTus(WebApplicationBuilder builder)
{
    builder.Services.AddTus()
        .AddControllerServices(config =>
        {
            config.AddController<MyTusController>();
            //config.AddEndpointServices();
        })
        .AddStorage("my-storage", new TusDiskStore(@"C:\tusfiles\"), true);
}