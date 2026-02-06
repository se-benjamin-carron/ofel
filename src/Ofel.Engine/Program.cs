using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ofel.Engine
{
    public static class ProgramHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) =>
                {
                    // enregistrer services core/engine
                    services.AddSingleton<ICalculationService, CalculationService>();
                    // charger plugins depuis dossier ./plugins
                    var pluginsPath = Path.Combine(AppContext.BaseDirectory, "plugins");
                    PluginLoader.LoadPlugins(services, pluginsPath);
                });
    }

    // Exemple très léger : interface de service de calcul
    public interface ICalculationService
    {
        void Run(string caseName);
    }

    public class CalculationService : ICalculationService
    {
        public void Run(string caseName)
        {
            Console.WriteLine($"Running calculation {caseName}");
        }
    }
}
