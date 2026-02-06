using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ofel.Plugins.Contracts
{
    public interface IPlugin
    {
        string Name { get; }
        Version Version { get; }
        /// <summary>
        /// Permet au plugin d'enregistrer ses services dans le conteneur DI de l'application.
        /// </summary>
        void ConfigureServices(IServiceCollection services);
    }
}
