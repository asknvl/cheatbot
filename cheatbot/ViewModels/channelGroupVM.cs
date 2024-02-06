using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class channelGroupVM : ViewModelBase
    {
        #region properties
        public string Name { get; set; }
        public int ID { get; set; }

        int channelSubscribes;
        public int ChannelSubscribes
        {
            get => channelSubscribes;
            set => this.RaiseAndSetIfChanged(ref channelSubscribes, value); 
        }
        #endregion
        public channelGroupVM()
        {

        }
    }
}
