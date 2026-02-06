using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Ofel.Plugins.Contracts;

namespace Ofel.Engine
{
    public static class PluginLoader
    {
        public static void LoadPlugins(IServiceCollection services, string pluginsPath)
        {
            if (!Directory.Exists(pluginsPath)) return;

            foreach (var dll in Directory.EnumerateFiles(pluginsPath, "*.dll"))
            {
                try
                {
                    var alc = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(dll), isCollectible: false);
                    var asm = alc.LoadFromAssemblyPath(Path.GetFullPath(dll));
                    var types = asm.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var t in types)
                    {
                        if (Activator.CreateInstance(t) is IPlugin plugin)
                        {
                            plugin.ConfigureServices(services);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log/ignorer : ne doit pas casser l'app
                    Console.WriteLine($"Plugin load failure {dll}: {ex.Message}");
                }
            }
        }
    }
}
