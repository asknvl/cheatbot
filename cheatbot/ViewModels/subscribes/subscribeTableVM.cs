using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.Models.server;
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
        #region vars
        List<dropVM> drops;
        CancellationTokenSource cts = null;
        IChannelsProvider chProvider;
        #endregion

        #region properties
        public ObservableCollection<channelVM> Channels { get; } = new();        
        public ObservableCollection<groupTitleVM> IDS { get; } = new();
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> subscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> unsubscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> refreshCmd { get; }
        #endregion

        public subscribeTableVM()
        {

            string url = "";
            string token = "";

            using (var db = new DataBaseContext())
            {
                AppSettings? found = db.AppSettings.FirstOrDefault();
                if (found != null)
                {
                    url = found.ChannelsURL;
                    token = found.ChannelsToken;
                }
            }

            chProvider = new ChannelsProvider(url, token);

            EventAggregator.getInstance().Subscribe(this);

            #region commands
            subscribeCmd = ReactiveCommand.CreateFromTask(async () => {

                if (cts != null)
                    return;

                cts = new CancellationTokenSource();
                List<Task> tasks = new();

                foreach (var channel in Channels)
                {
                    foreach (var group in channel.Selection.SelectedItems)
                    {
                        tasks.Add(Task.Run(async () => {
                            await group.Subscribe(channel, cts);
                        }));                        
                    }
                }

                await Task.WhenAll(tasks);

                cts = null;
                refresh();

            });

            unsubscribeCmd = ReactiveCommand.CreateFromTask(async () => {
                if (cts != null)
                    return;

                cts = new CancellationTokenSource();
                List<Task> tasks = new();

                foreach (var channel in Channels)
                {
                    foreach (var group in channel.Selection.SelectedItems)
                    {
                        tasks.Add(Task.Run(async() => {
                            await group.Unsubscribe(channel, cts);
                        }));
                    }
                }

                await Task.WhenAll(tasks);

                cts = null;
                refresh();

            });

            stopCmd = ReactiveCommand.Create(() => {                 
                cts?.Cancel();
            });

            refreshCmd = ReactiveCommand.Create(() => {
                refresh();
            });

            
            #endregion
        }

        #region helpers
        public void refresh()
        {
            foreach (var channel in Channels)
                channel.Update();

            using (var db = new DataBaseContext())
            {
                var groups = db.Groups.ToList();
                refreshTitle(groups);
            }
        }
        void refreshTitle(List<GroupModel> groups)
        {
            IDS.Clear();
            foreach (var g in groups)
            {
                var title = new groupTitleVM();
                title.ID = "G" + g.id;
                title.TotalDropsInGroup = drops.Where(d => d.group_id == g.id).Count();
                IDS.Add(title);
            }
        }
        async Task loadChannels()
        {

            await Task.Run(async () =>
            {

                //using (var db = new DataBaseContext())
                //{
                //    var channels = db.Channels.ToList();
                //    var groups = db.Groups.ToList();

                //    refreshTitle(groups);

                //    foreach (var channel in channels)
                //    {

                //        var found = Channels.FirstOrDefault(c => c.TG_id == channel.tg_id);
                //        if (found == null)
                //        {
                //            var ch = new channelVM(channel, groups, drops);
                //            Channels.Add(ch);
                //        }
                //    }
                //}

                using (var db = new DataBaseContext())
                {
                    var groups = db.Groups.ToList();
                    var channels = await chProvider.GetChannels();                    

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

                    foreach (var ch in Channels)
                        ch.Update(drops);

                    await loadChannels();
                    refresh();
                    break;
            }
        }


    }
}
