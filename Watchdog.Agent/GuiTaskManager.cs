using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;

namespace Watchdog.Agent;

public class GuiTaskManager(ILogger<GuiTaskManager> logger)
{
    private readonly ILogger<GuiTaskManager> _logger = logger;

    /// <summary>
    /// Creates a Task Scheduler task to run the GUI application
    /// if it does not already exist.
    /// </summary>
    /// <param name="service">Service information including process name and exe path</param>
    public string EnsureGuiTaskExists(ServiceInfo service)
    {
        var taskName = $"TaskScheduler_{service.ProcessName}";
        using var ts = new TaskService();

        var existingTask = ts.FindTask(taskName, true);
        if (existingTask != null)
        {
            _logger.LogInformation($"Task '{taskName}' already exists. Skipping creation.");
            return taskName;
        }

        var task = ts.NewTask();
        task.RegistrationInfo.Description = service.Decsription;

        task.Triggers.Add(new LogonTrigger());
        task.Actions.Add(new ExecAction($"\"{service.ExePath }\""));
        task.Principal.LogonType = TaskLogonType.InteractiveToken;

        ts.RootFolder.RegisterTaskDefinition(taskName, task);

        _logger.LogInformation($"Task '{taskName}' has been successfully created.");

        return taskName;
    }

    /// <summary>
    /// Run the GUI task via Task Scheduler
    /// </summary>
    public void RunGuiTask(string taskName)
    {
        try
        {
            Process.Start("schtasks.exe", "/run /tn " + taskName);
            _logger.LogInformation($"Task '{taskName}' started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to start task '{taskName}': {ex.Message}");
        }
    }
}
