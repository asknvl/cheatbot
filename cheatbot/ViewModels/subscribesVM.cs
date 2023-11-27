using asknvl.logger;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class subscribesVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        ILogger logger;
        List<int> runningGroups = new();
        #endregion

        #region properties
        List<GroupModel> groups;
        public List<GroupModel> Groups
        {
            get => groups;
            set
            {
                this.RaiseAndSetIfChanged(ref groups, value);
            }
        }

        GroupModel selectedGroup;
        public GroupModel SelectedGroup
        {
            get => selectedGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedGroup, value);
                updateGroupSubscribes();
            }
        }

        public ObservableCollection<channelVM> GroupSubscribes { get; } = new();
        

        channelVM selectedSubscribe;
        public channelVM SelectedSubscribe
        {
            get => selectedSubscribe;
            set => this.RaiseAndSetIfChanged(ref selectedSubscribe, value);
        }

        public ObservableCollection<channelVM> ChannelList { get; } = new();

        channelVM selectedChannel;
        public channelVM SelectedChannel
        {
            get => selectedChannel;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedChannel, value);                
            }
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> subscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> unsubscribeCmd { get; }
        #endregion

        public subscribesVM(ILogger logger) {

            EventAggregator.getInstance().Subscribe(this);

            update();

            subscribeCmd = ReactiveCommand.Create(() => {

                if (SelectedChannel == null)
                    return;

                if (!runningGroups.Contains(SelectedGroup.id))
                    return;

                using (var db = new DataBaseContext())
                {
                    var found = db.GroupSubscribes.FirstOrDefault(gs => gs.group_id == SelectedGroup.id && gs.channel_id == SelectedChannel.id);
                    if (found == null)
                    {
                        var subscribeMessage = new ChannelSubscribeRequestEventMessage(SelectedGroup.id, SelectedChannel.link);
                        EventAggregator.getInstance().Publish((BaseEventMessage)subscribeMessage);

                        var subscribeModel = new GroupSubscribeModel()
                        {
                            group_id = SelectedGroup.id,
                            channel_id = SelectedChannel.id
                        };

                        db.GroupSubscribes.Add(subscribeModel);
                        db.SaveChanges();
                        
                        updateGroupSubscribes();                             
                    }

                }

            });
            unsubscribeCmd = ReactiveCommand.Create(() => {

                if (SelectedSubscribe == null)
                    return;

                if (!runningGroups.Contains(SelectedGroup.id))
                    return;

                using (var db = new DataBaseContext())
                {
                    var found = db.GroupSubscribes.FirstOrDefault(gs => gs.group_id == SelectedGroup.id && gs.channel_id == SelectedSubscribe.id);
                    if (found != null)
                    {
                        db.GroupSubscribes.Remove(found);
                        db.SaveChanges();
                    }
                }

                var unsubscribeMessage = new ChannelUnsubscribeRequestEventMessage(SelectedGroup.id, SelectedSubscribe.tg_id);
                EventAggregator.getInstance().Publish((BaseEventMessage)unsubscribeMessage);

                updateGroupSubscribes();

            });

        }

        #region private
        async Task loadGroups()
        {
            await Task.Run(() => {
                using (var db = new DataBaseContext())
                {
                    Groups = db.Groups.ToList();
                    if (Groups.Count > 0)
                        SelectedGroup = Groups[0];
                }
            });
        }

        async Task updateChannels()
        {
            await Task.Run(() => { 
            
                using (var db = new DataBaseContext())
                {
                    var channels = db.Channels.ToList();
                    ChannelList.Clear();
                    foreach (var channel in channels)
                    {
                        ChannelList.Add(new channelVM(channel));
                    }
                }
            });
        }

        void updateGroupSubscribes()
        {
            using (var db = new DataBaseContext())
            {
                var subscribes = db.GroupSubscribes.Where(s => s.group_id == SelectedGroup.id).ToList();
                var channels = db.Channels.ToList();
                                
                GroupSubscribes.Clear();

                foreach (var sub in subscribes)
                {
                    var ch = channels.FirstOrDefault(c => c.id == sub.channel_id);
                    if (ch != null)
                    {                        
                        GroupSubscribes.Add(new channelVM(ch));
                    }
                }
            }
        }

        void update()
        {
            Task.Run(async () => {
                await loadGroups();
                await updateChannels();
                
            });
        }

        long tg_id_prev;
        public void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case ChannelListUpdateRequestEventMessage chanelUpdateMessage:

                    if (chanelUpdateMessage.channel_id == tg_id_prev)
                        return;

                    tg_id_prev = chanelUpdateMessage.channel_id;

                    updateChannels();

                    break;

                case GroupStartedEventMessage groupStartMessage:
                    if (!runningGroups.Contains(groupStartMessage.group_id))
                        runningGroups.Add(groupStartMessage.group_id);
                    break;

                case GroupStoppedEventMessage groupStopMessage:
                    if (runningGroups.Contains(groupStopMessage.group_id))
                        runningGroups.Remove(groupStopMessage.group_id);
                    break;
            }
        }
        #endregion

    }
}
