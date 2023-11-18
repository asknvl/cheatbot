using cheatbot.Database;
using cheatbot.Database.models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class channelVM : ViewModelBase
    {
        #region properties
        string? _geotag;
        public string? geotag
        {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);
        }

        string? _link;
        public string? link
        {
            get => _link;
            set => this.RaiseAndSetIfChanged(ref _link, value);
        }

        string? _name;
        public string? name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        long _tg_id;
        public long tg_id
        {
            get => _tg_id;
            set => this.RaiseAndSetIfChanged(ref _tg_id, value);    
        }
        #endregion

        public channelVM(ChannelModel model) 
        {
            geotag = model.geotag;
            link = model.link;    
            tg_id = model.tg_id;
            name = model.name;
        }
    }
}
