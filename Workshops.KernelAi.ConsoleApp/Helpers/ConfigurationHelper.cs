using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Workshops.KernelAi.ConsoleApp.Helpers;

public static class ConfigurationHelper
{
    public static WorkshopSettings LoadWorkshopSettings(params string[] args)
    {
        ConfigurationBuilder configurationBuilder = new();
        IConfiguration config = configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .AddCommandLine(args)
            .Build();

        return config.Get<WorkshopSettings>() ?? throw new InvalidOperationException("Failed to load WorkshopSettings from configuration.");
    }
}
