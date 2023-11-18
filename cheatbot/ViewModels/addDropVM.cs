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
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> okCmd {  get; set; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; set; }  
        #endregion

        public addDropVM()
        {
            okCmd = ReactiveCommand.Create(() => {
                AddDropRequestEvent?.Invoke(phone_number);
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Close();
            });
        }

        public event Action<string> AddDropRequestEvent;        
    }
}
