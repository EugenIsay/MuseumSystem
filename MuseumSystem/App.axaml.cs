using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MuseumSystem.Interfaces;

namespace MuseumSystem
{
    public partial class App : Application
    {
        public static INativeMediaPlayerService AppNativeVideoPlayerService { get; set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new AuthorizationWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}