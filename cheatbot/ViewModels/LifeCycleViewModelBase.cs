using Avalonia.Logging;
using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class LifeCycleViewModelBase : ViewModelBase
    {
        #region public
        public virtual void OnStarted()
        {         
        }

        public virtual void OnStopped()
        {         
            StopetEvent?.Invoke();
        }
        public void Close()
        {
            CloseRequestEvent?.Invoke();
        }
        #endregion

        #region callbacks
        public event Action CloseRequestEvent;
        public event Action StopetEvent;
        #endregion
    }
}
