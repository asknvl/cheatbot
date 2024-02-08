using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.subscribes
{
    public class subscribeTableVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region properties
        List<dropVM> drops;
        CancellationTokenSource cts = null;
        #endregion

        #region properties
        public ObservableCollection<channelVM> Channels { get; } = new();        
        #endregion

        public subscribeTableVM()
        {

            EventAggregator.getInstance().Subscribe(this);

            #region commands
            subscribeCmd = ReactiveCommand.CreateFromTask(async () => {

                if (cts != null)
                    return;

                cts = new CancellationTokenSource();

                foreach (var channel in Channels)
                {
                    foreach (var group in channel.Selection.SelectedItems)
                    {
                        await group.Subscribe(channel, cts);
                    }
                }

                cts = null;

            });
            unsubscribeCmd = ReactiveCommand.CreateFromTask(async () => {                
            });



            refreshCmd = ReactiveCommand.CreateFromTask(async () => {

                foreach (var channel in Channels)
                    channel.Update();

            });                
            #endregion
        }

        #region commands
        public ReactiveCommand<Unit, Unit> subscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> unsubscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> refreshCmd { get; }
        #endregion

        #region helpers
        async Task loadChannels()
        {

            await Task.Run(() =>
            {              

                using (var db = new DataBaseContext())
                {
                    var channels = db.Channels.ToList();
                    var groups = db.Groups.ToList();

                    foreach (var channel in channels)
                    {

                        var found = Channels.FirstOrDefault(c => c.TG_id == channel.tg_id);
                        if (found == null)
                        {
                            var ch = new channelVM(channel, groups, drops);
                            Channels.Add(ch);
                        }
                    }
                }
            });
        }
        #endregion

        public async void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case DropListUpdatedEventMessage dluem:
                    drops = dluem.drop_list.ToList();
                    await loadChannels();
                    break;
            }
        }


    }
}
