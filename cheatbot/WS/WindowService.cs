using Avalonia.Controls;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using cheatbot.ViewModels;
using cheatbot.Views;

namespace cheatbot.WS
{
    public class WindowService : IWindowService
    {
        #region vars
        static WindowService instance;
        List<Window> windowList = new();
        Window mainWindow;
        #endregion

        private WindowService()
        {
        }

        #region public
        public static WindowService getInstance()
        {
            if (instance == null)
                instance = new WindowService();
            return instance;
        }

        public void ShowDialog(LifeCycleViewModelBase vm)
        {
            Window wnd = null;
            switch (vm)
            {

            }

            vm.CloseRequestEvent += async () =>
            {
                if (wnd != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        wnd.Close();
                    });
                }
            };

            wnd.DataContext = vm;
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            wnd.ShowDialog(mainWindow);
            
        }

        public void ShowWindow(LifeCycleViewModelBase vm)
        {
            Window wnd = null;

            switch (vm)
            {
                case mainVM:
                    wnd = new mainWnd();
                    break;                
            }

            mainWindow = wnd;
            wnd.Closing += (s, e) => {

                var found = windowList.FirstOrDefault(v => v.GetType().Equals(wnd.GetType()));
                if (found != null)
                    windowList.Remove(found);

                vm.OnStopped();
            };

            vm.CloseRequestEvent += () =>
            {
                if (wnd != null)
                {
                    wnd.Close();
                }
            };


            Window found = windowList.FirstOrDefault(v => v.GetType().Equals(wnd.GetType()));
            if (found == null)
            {
                wnd.DataContext = vm;
                wnd.Show();
                windowList.Add(wnd);
                vm.OnStarted();
            }
            else
                found.Activate();

        }
        #endregion
    }
}
