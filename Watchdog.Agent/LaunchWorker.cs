using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Watchdog.Agent;

public class LaunchWorker(ILogger<LaunchWorker> logger, IOptions<WatchdogOptions> options, GuiTaskManager guiTaskManager) : BackgroundService
{
    private readonly ILogger<LaunchWorker> _logger = logger;
    private readonly GuiTaskManager _guiTaskManager = guiTaskManager;
    private readonly WatchdogOptions _options = options.Value;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        _logger.LogInformation("{worker} started.", _options.Name);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("{worker} stopped.", _options.Name);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _options.WatchList
            .Select(item => MonitorProcessAsync(item, stoppingToken))
            .ToList();

        return Task.WhenAll(tasks);
    }

    private async Task MonitorProcessAsync(ServiceInfo service, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Process '{process}' in progress", service.ProcessName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!File.Exists(service.ExePath))
                {
                    _logger.LogError("File '{exe}' not found.", service.ExePath);
                }
                else
                {
                    var runningProcesses = Process.GetProcessesByName(service.ProcessName);

                    if (runningProcesses.Length == 0)
                    {
                        try
                        {
                            _logger.LogInformation("Process '{process}' is not running. Attempting to start...", service.ProcessName);

                            if (service.IsUIProcess == true)
                            {
                                var taskName = _guiTaskManager.EnsureGuiTaskExists(service);
                                _guiTaskManager.RunGuiTask(taskName);
                            }
                            else
                            {
                                Process.Start(service.ExePath);
                            }

                            _logger.LogInformation("Process '{process}' started successfully.", service.ProcessName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to start process '{process}'.", service.ProcessName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring process '{process}'.", service.ProcessName);
                _logger.LogInformation(
                    "Process '{process}' will retry in {seconds} seconds.",
                    service.ProcessName,
                    service.CheckIntervalAfterException / 1000
                );

                await Task.Delay(TimeSpan.FromMilliseconds(service.CheckIntervalAfterException), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(service.CheckInterval), stoppingToken);
        }

        _logger.LogInformation("Process '{process}' stopped.", service.ProcessName);
    }
}
