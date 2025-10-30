dotnet new sln -n Ofel
dotnet new classlib -n Ofel.Plugins.Contracts -f net7.0
dotnet new classlib -n Ofel.Engine -f net7.0
# Installer les templates Avalonia si n�cessaire:
# dotnet new -i Avalonia.Templates
dotnet new avalonia.app -n Ofel.UI -f net7.0
# Ajouter au solution
dotnet sln add src/ofel.Core/ofel.Core.csproj
dotnet sln add src/Ofel.Plugins.Contracts/Ofel.Plugins.Contracts.csproj
dotnet sln add src/Ofel.Engine/Ofel.Engine.csproj
dotnet sln add src/Ofel.UI/Ofel.UI.csproj
# R�f�rences projets
dotnet add src/Ofel.Engine/Ofel.Engine.csproj reference src/ofel.Core/ofel.Core.csproj
dotnet add src/Ofel.UI/Ofel.UI.csproj reference src/Ofel.Engine/Ofel.Engine.csproj
dotnet add src/Ofel.Plugins.Sample/Ofel.Plugins.Sample.csproj reference src/Ofel.Plugins.Contracts/Ofel.Plugins.Contracts.csprojusing System;
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