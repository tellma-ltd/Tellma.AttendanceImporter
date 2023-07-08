using Tellma.AttendanceImporter;
using Tellma.AttendanceImporter.Samsung;
using Tellma.AttendanceImporter.WinService;
using Tellma.AttendanceImporter.Zkem;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(config =>
    {
        config.ServiceName = "Tellma Attendance Importer";
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>(); // automatically defines Ilogger
        services.Configure<ImporterOptions>(hostContext.Configuration);
        services.Configure<TellmaOptions>(hostContext.Configuration.GetSection("Tellma"));
        services.AddScoped<ZkemDeviceService>();
        services.AddScoped<SamsungDeviceService>();
        services.AddScoped<TellmaAttendanceImporter>(); // with every new scope, a new instance
        services.AddScoped<IDeviceServiceFactory, DeviceServiceFactory>();//DI to return the proper factory
    })
    .ConfigureLogging((hostContext, loggingBuilder) =>
    {
        loggingBuilder.AddDebug();
        loggingBuilder.AddEmail(hostContext.Configuration);
    })
    .Build();

host.Run();
