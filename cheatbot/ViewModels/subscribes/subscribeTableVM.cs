using Avalonia.Controls;
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

            chProvider = ChannelsProvider.getInstance();

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
                        
            refreshCmd = ReactiveCommand.CreateFromTask(async () => {

                await loadChannels();

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


        List<ChannelModel> channels_prev = new();
        async Task loadChannels()
        {

            await Task.Run(async () =>
            {

                //await Dispatcher.UIThread.InvokeAsync(() =>
                //{
                //    channelVM.ClearUniqs();
                //    Channels.Clear();
                //});

                using (var db = new DataBaseContext())
                {
                    var groups = db.Groups.ToList();
                    var channels = await chProvider.GetChannels();


                    foreach (var channel in channels)
                    {
                        var found = Channels.FirstOrDefault(c => c.TG_id == channel.tg_id);
                        if (found == null)
                            Channels.Add(new channelVM(channel, groups, drops));
                        else
                        {
                            found.Name = channel.geotag;
                            found.Link = channel.link;                            
                        }

                    }

                    var difference = channels.Except(channels_prev).ToList();
                    channels_prev = channels.ToList();

                    //foreach (var channel in difference)
                    //{
                    //    Channels.Add(new channelVM(channel, groups, drops));                                            
                    //}

                    //Удаление старых каналов, которых нет в текущем списке
                    var channelsToRemove = Channels.Where(c => !channels.Any(ch => ch.tg_id == c.TG_id)).ToList();
                    foreach (var channel in channelsToRemove)
                    {
                        Channels.Remove(channel);
                    }


                    //foreach (var channel in difference)
                    //{

                    //    var found = Channels.FirstOrDefault(c => c.TG_id == channel.tg_id);
                    //    if (found == null)
                    //    {
                    //        var ch = new channelVM(channel, groups, drops);
                    //        Channels.Add(ch);
                    //    }
                    //}
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
