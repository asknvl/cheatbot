using cheatbot.Database;
using cheatbot.Database.models;
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
    public class channelVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
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

        uint _wievedMessagesCounter = 0;
        public uint viewedMessagesCounter
        {
            get => _wievedMessagesCounter;
            set => this.RaiseAndSetIfChanged(ref _wievedMessagesCounter, value);
        }
        #endregion

        public channelVM(ChannelModel model) 
        {
            geotag = model.geotag;
            link = model.link;    
            tg_id = model.tg_id;
            name = model.name;

            EventAggregator.getInstance().Subscribe(this);
        }

        public void OnEvent(BaseEventMessage message)
        {

            switch (message)
            {
                case ChannelMessageViewedEventMessage messageWieved:
                    if (messageWieved.channel_id == tg_id)
                        viewedMessagesCounter++;
                    break;
            }            
        }
    }
}
