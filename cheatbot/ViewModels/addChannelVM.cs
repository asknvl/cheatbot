using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class addChannelVM : SubContentVM
    {
        #region properties
        string _geotag;
        public string geotag
        {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);
        }

        string _link;
        public string link
        {
            get => _link;
            set => this.RaiseAndSetIfChanged(ref _link, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> okCmd { get; set; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; set; }
        #endregion

        public addChannelVM()
        {
            okCmd = ReactiveCommand.Create(() => {
                AddChannelRequestEvent?.Invoke(geotag, link);
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Close();
            });
        }

        public event Action<string, string> AddChannelRequestEvent;
    }
}
