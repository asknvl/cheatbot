using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class addDropVM : SubContentVM
    {
        #region properties
        string _phone_number;
        public string phone_number
        {
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string __2fa_password = string.Empty;
        public string _2fa_password
        {
            get => __2fa_password;
            set => this.RaiseAndSetIfChanged(ref __2fa_password, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> okCmd {  get; set; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; set; }  
        #endregion

        public addDropVM()
        {
            okCmd = ReactiveCommand.Create(() => {
                AddDropRequestEvent?.Invoke(phone_number, _2fa_password);
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Close();
            });
        }

        public event Action<string, string> AddDropRequestEvent;        
    }
}
