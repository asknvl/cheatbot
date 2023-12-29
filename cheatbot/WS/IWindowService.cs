using cheatbot.ViewModels;
using System.Threading.Tasks;

namespace cheatbot.WS
{
    public interface IWindowService
    {
        void ShowWindow(LifeCycleViewModelBase vm);
        void ShowDialog(LifeCycleViewModelBase vm);
        Task<string?> ShowFolderDialog();
    }
}
