using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ofel.Engine;
using Ofel.UI.ViewModels;

namespace Ofel.UI
{
    public class App : Application
    {
        private IHost _host;
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            _host = ProgramHost.CreateHostBuilder(Array.Empty<string>()).Build();
            _host.Start();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = _host.Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow { DataContext = vm };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}