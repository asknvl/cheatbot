using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class subscribesVM : ViewModelBase
    {
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

        List<channelVM> groupSubscribes;
        public List<channelVM> GroupSubscribes
        {
            get => groupSubscribes;
            set => this.RaiseAndSetIfChanged(ref groupSubscribes, value);
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

        public subscribesVM() {

            update();

            subscribeCmd = ReactiveCommand.Create(() => {

                var subscribeMessage = new ChannelSubscribeRequestEventMessage(SelectedGroup.id, SelectedChannel.link);
                EventAggregator.getInstance().Publish((BaseEventMessage)subscribeMessage);

            });
            unsubscribeCmd = ReactiveCommand.Create(() => {
                var unsubscribeMessage = new ChannelUnsubscribeRequestEventMessage(SelectedGroup.id, SelectedChannel.tg_id);
                EventAggregator.getInstance().Publish((BaseEventMessage)unsubscribeMessage);
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

        async Task updateGroupSubscribes()
        {
            await Task.Run(() => {

                //using (var db = new DataBaseContext())";````                                                                                                                                          `````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                //{
                //    var chanhelIds = db.GroupSubscribes.Where(gs => gs.group_id == SelectedGroup.id).Select(c => c.channel_id).ToList();                    
                //}
            
            });
        }

        async Task updateChannels()
        {
            await Task.Run(() => { 
            
                using (var db = new DataBaseContext())
                {
                    var channesl = db.Channels.ToList();
                    ChannelList.Clear();
                    foreach (var channel in channesl)
                    {
                        ChannelList.Add(new channelVM(channel));
                    }
                }
            });
        }

        void update()
        {
            Task.Run(async () => {
                await loadGroups();
                await updateChannels();
            });
        }
        #endregion

    }
}
