using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using cheatbot.ViewModels;
using cheatbot.Views;
using cheatbot.WS;

namespace cheatbot
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                mainVM main = new mainVM();
                WindowService.getInstance().ShowWindow(main);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
