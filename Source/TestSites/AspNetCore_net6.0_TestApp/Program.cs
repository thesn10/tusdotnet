using AspNetCore_net6._0_TestApp;
using AspNetCore_net6._0_TestApp.Authentication;
using AspNetCore_net6._0_TestApp.Endpoints;
using AspNetCore_net6._0_TestApp.Services;
using Microsoft.AspNetCore.Authentication;
using tusdotnet.ExternalMiddleware.EndpointRouting;
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

app.MapTusController<MyTusController>("/files");
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
    // to use tus controllers
    builder.Services.AddMvcCore();

    builder.Services.AddTus().AddController<MyTusController>().AddStorage("my-storage", new TusDiskStore(@"C:\tusfiles\"), true).AddEndpointServices();
}