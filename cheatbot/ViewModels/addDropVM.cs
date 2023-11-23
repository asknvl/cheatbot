using cheatbot.ViewModels.events;
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

        int _group_id;
        public int group_id
        {
            get => _group_id;
            set => this.RaiseAndSetIfChanged(ref this._group_id, value);
        }

        string __2fa_password = "Wf52";
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

        public addDropVM(int group_id)
        {
            this.group_id = group_id;

            okCmd = ReactiveCommand.Create(() => {
                //AddDropRequestEvent?.Invoke(phone_number, _2fa_password);
                EventAggregator.getInstance().Publish((BaseEventMessage)new AddDropEventMessage(phone_number, _2fa_password, group_id));
            });

            cancelCmd = ReactiveCommand.Create(() => {
                Close();
            });
        }

        public event Action<string, string> AddDropRequestEvent;        
    }
}
