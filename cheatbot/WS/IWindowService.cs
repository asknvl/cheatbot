using cheatbot.ViewModels;

namespace cheatbot.WS
{
    public interface IWindowService
    {
        void ShowWindow(LifeCycleViewModelBase vm);
        void ShowDialog(LifeCycleViewModelBase vm);
    }
}
