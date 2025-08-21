using Serilog;
using Watchdog.Agent;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()    
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddSerilog()
    .Configure<WatchdogOptions>(builder.Configuration.GetSection("Watchdog"))
    .AddWindowsService(options => options.ServiceName = "Watchdog Agent Service");

builder.Services.AddSingleton<GuiTaskManager>();
builder.Services.AddHostedService<LaunchWorker>();

var host = builder.Build();

host.Run();
