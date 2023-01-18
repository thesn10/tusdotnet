# tusdotnet

[![NuGet](https://img.shields.io/nuget/v/tusdotnet.svg?color=blue&style=popout-square)](https://www.nuget.org/packages/tusdotnet) [![NuGet](https://img.shields.io/nuget/dt/tusdotnet.svg?color=blue&style=popout-square)](https://www.nuget.org/packages/tusdotnet) [![codecov](https://img.shields.io/codecov/c/github/tusdotnet/tusdotnet.svg?color=blue&style=popout-square)](https://codecov.io/gh/tusdotnet/tusdotnet)

>"Our aim is to solve the problem of unreliable file uploads once and for all. tus is a new open protocol for resumable uploads built on HTTP. It offers simple, cheap and reusable stacks for clients and servers. It supports any language, any platform and any network." - https://tus.io

tusdotnet is a .NET server implementation of the tus.io protocol that runs on both .NET 4.x and .NET Core!

This branch is intended for the new [tus2 protocol](https://github.com/tus/tus-v2). For tus 1.x, see the main branch: https://github.com/tusdotnet/tusdotnet/

Please note that this is a POC/experimental implementation and breaking changes will happen.

## Install

Clone this branch and include it in your project. All classes related to tus2 are found in the `tusdotnet.tus2` namespace. Files are found in `Source/tusdotnet/tus2`.

## Configure (simple)

In Startup.cs add the following:

```csharp

public void ConfigureServices(IServiceCollection services)
{
    services.AddTus2(options =>
    {
        // Shorthand for adding a scoped implementation of Tus2DiskStorage to the DI container
        options.AddDiskStorage(@"C:\path\to\save\files");

        // Adds MyTusHandler as transient
        options.AddHandler<MyTusHandler>();
    });
}

public void Configure(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapTus2<MyTusHandler>("/files-tus-2");
    });
}

```

Define a class called `MyTusHandler` that inherits from `tusdotnet.Tus2.TusHandler` and override the methods you would like to handle. The `TusHandler` base class will handle communication with the storage so remember to call the base implementation in your override. Note that one does not need to override all methods, just the ones one wishes to handle differently than the default behavior. In most cases it is enough to override the `FileComplete` method which is called when the upload is complete.

```csharp
public class MyTusHandler : TusHandler
{
    private readonly ILogger _logger;
    private readonly Tus2StorageFacade _storage;

    public MyTusHandler(ILoggerFactory loggerFactory, Tus2StorageFacade storage)
        : base(storage)
    {
        _logger = loggerFactory.CreateLogger(nameof(MyTusHandler));
        _storage = storage;
    }

    public override bool AllowClientToDeleteFile => true;

    public override async Task<CreateFileProcedureResponse> CreateFile(CreateFileContext context)
    {
        _logger.LogInformation("Creating file {UploadToken}", context.Headers.UploadToken);

        var response = await _storage.CreateFile(context);

        _logger.LogInformation("File created? {Success}", response.Status == System.Net.HttpStatusCode.Created);

        return response;
    }

    public override async Task<UploadTransferProcedureResponse> WriteData(WriteDataContext context)
    {
        _logger.LogInformation("Receiving upload, starting at {UploadOffset}", context.Headers.UploadOffset);

        var response = await base.WriteData(context);

        _logger.LogInformation("Was success? {Success}", response.Status == System.Net.HttpStatusCode.Created);

        return response;
    }

    public override async Task<UploadRetrievingProcedureResponse> RetrieveOffset(RetrieveOffsetContext context)
    {
        _logger.LogInformation("Retrieving offset for {UploadToken}", context.Headers.UploadToken);

        var response = await base.RetrieveOffset(context);

        _logger.LogInformation("Offset is {UploadOffset}", response.UploadOffset);

        return response;
    }

    public override async Task<UploadCancellationProcedureResponse> Delete(DeleteContext context)
    {
        _logger.LogInformation("Deleting file {UploadToken}", context.Headers.UploadToken);

        var response = await base.Delete(context);

        _logger.LogInformation("File deleted? {Deleted}", response.Status == System.Net.HttpStatusCode.NoContent);

        return response;
    }

    public override Task FileComplete(FileCompleteContext context)
    {
        _logger.LogInformation("File {UploadToken} is complete", context.Headers.UploadToken);

        return base.FileComplete(context);
    }
}
```

## Configure storage (more complex)

The tus2 implementation also supports creating storage instances using a factory. The factory supports creating "named storage" which allows to separate different storage options into different instances similar to HttpClientFactory.

```csharp
services.AddTus2(options =>
{
    // SimpleTus2StorageFactory being a class implementing ITus2StorageFactory. 
    // Same as adding a scoped instance of <ITus2StorageFactory, SimpleTus2StorageFactory()>
    options.AddStorageFactory(new SimpleTus2StorageFactory());
    options.AddHandler<MyTusHandler>();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapTus2<MyTusHandler>("/files-tus-2");
});

// Handler constructor needs to be updated to use the storage factory and a possible name of the storage.
public class MyTusHandler : TusHandler
{
    public MyTusHandler(ITus2ConfigurationManager config)
        : base(config, "MyStorage") // "MyStorage" is optional and will provide the string "MyStorage" to the factory.
    {
    }

    ...
}
```

## Configure ongoing upload manager (more complex)

In tus2 locks are not used. Instead all previous upload requests for a single `Upload-Token` must be terminated when a new request for the same `Upload-Token` is received. In tusdotnet this is handled by the `IOngoingUploadManager`. By default, an `OngoingUploadManagerInMemory` instance will be used. If you run your setup in a cluster you will need to switch to either `OngoingUploadManagerDiskBased` and point it to a shared disk or implement your own.

```csharp
services.AddTus2(options =>
{
    // Add disk based ongoing upload manager as a scoped instance.
    options.AddDiskBasedUploadManager(@"C:\tusfiles");
    // The above is the same as calling:
    options.AddUploadManager(new OngoingUploadManagerDiskBased(new() { SharedDiskPath = @"C:\tusfiles" }));

    // OR use your own:

    // Add an instance as a scoped instance...
    options.AddUploadManager(new RedisOngoingUploadManager("connection string"));
    // ... or add ongoing upload manager factory as a scoped instance.
    builder.AddUploadManagerFactory(new RedisOngoingUploadManagerFactory());
});

```

## How do I...? 

### Run the tus2 implementation in a cluster/on multiple machines?
Register the `OngoingUploadManagerDiskBased` in your DI and tusdotnet will automatically solve the new locking behavior. You can also implement your own implementation of `IOngoingUploadManager` and use that.

### How do I access the storage outside my tus handler?

When adding tus2 to your DI container the following is added:
* Tus2Storage instance (if `builder.AddStorage` is used)
* Tus2StorageFacade instance (if `builder.AddStorage` is used)
* Any factories registered (both for storage and ongoing upload manager)
* ITus2ConfigurationManager instance which can grab the storage and ongoing upload manager

Tus2StorageFacade is a wrapper around Tus2Storage which makes is easier to work with the entire tus2 flow instead of just calling methods directly on the storage.

```csharp

services.AddTus2(options =>
{
    // Defaults
    options.AddDiskStorage(@"C:\tusfiles");
    options.AddUploadManager(new OngoingUploadManagerDiskBased(new() { SharedDiskPath = System.IO.Path.GetTempPath() }));

    // Storage factory
    options.AddStorageFactory(new SimpleTus2StorageFactory());
});

public class MyService
{
    private readonly ITus2ConfigurationManager _config;
    private readonly Tus2Storage _defaultStorage;
    private readonly Tus2StorageFacade _defaultStorageFacade;
    private readonly IOngoingUploadManager _defaultUploadManager;

    public MyService(
        ITus2ConfigurationManager config,
        IOngoingUploadManager defaultUploadManager,
        Tus2StorageFacade facade,
        TUs2Storage storage)
    {
        _config = config;
        _defaultUploadManager = defaultUploadManager;
        _defaultStorageFacade = facade;
        _defaultStorage = storage;
    }

    public async Task MyMethod()
    {
        var defaultStorage = await _config.GetDefaultStorage();
        var defaultStorage2 = await _config.GetDefaultStorage();

        // Calls SimpleTus2StorageFactory.CreateNamedStorage with name "MyProfile".
        var myProfileStorage = await _config.GetNamedStorage("MyProfile");

        var defaultUploadManager = await _config.GetDefaultUploadManager();

        // True, the storage factory is only called once per scope.
        Assert.AreEqual(defaultStorage, defaultStorage2);

        // True
        Assert.AreEqual(defaultUploadManager, _defaultUploadManager);

        // True, the facade is just a wrapper around the storage
        Assert.AreEqual(_defaultStorageFacade.Storage, _defaultStorage);
    }
}
```

* ASP.NET Core 6 (.NET 6.0)
* ASP.NET Core 3.1 (.NET Core 3.1)
* ASP.NET Core 3.1 (.NET Core 3.1)
* ASP.NET Core 3.0 (.NET Core 3.0)
* ASP.NET Core 2.2 (.NET Core 2.2)
* ASP.NET Core 2.2 (.NET Framework 4.6.2)
* ASP.NET Core 2.1 (.NET Core 2.1)
* OWIN (.NET Framework 4.5.2)

## Test sites

Test site only is available for ASP.NET Core 3.1 (.NET Core 3.1) as the tus2 implementation requires .NET classes only found in Core 3.1 and later.

## License

This project is licensed under the MIT license, see [LICENSE](LICENSE).

## Want to know more?

Discussion can be held in this issue: https://github.com/tusdotnet/tusdotnet/issues/164