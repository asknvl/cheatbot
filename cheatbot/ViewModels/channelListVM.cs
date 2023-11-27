using asknvl.logger;
using cheatbot.Database;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using DynamicData;

namespace cheatbot.ViewModels
{
    public class channelListVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars        
        #endregion

        #region properties
        public ObservableCollection<channelVM> ChannelList { get; } = new();

        channelVM selectedChannel;
        public channelVM SelectedChannel
        {
            get => selectedChannel;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedChannel, value);
                SubContent = selectedChannel;
            }
        }

        object subcontent;
        public object SubContent
        {
            get => subcontent;
            set => this.RaiseAndSetIfChanged(ref subcontent, value);    
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> deleteCmd { get; }
        //public ReactiveCommand<Unit, Unit> subscribeCmd { get; }
        //public ReactiveCommand<Unit, Unit> unsubscribeCmd { get; }
        #endregion

        public channelListVM()
        {           

            updateList();

            addCmd = ReactiveCommand.Create(() => {

                addChannelVM addVM = new addChannelVM();
                SubContent = addVM;

                addVM.AddChannelRequestEvent += (geotag, link) => {

                    if (string.IsNullOrEmpty(geotag) || string.IsNullOrEmpty(link))
                        return;

                    using (var db = new DataBaseContext())
                    {
                        var found = db.Channels.FirstOrDefault(c => c.link.Equals(link) || c.geotag.Equals(geotag));

                        if (found == null)
                        {
                            var channelModel = new ChannelModel()
                            {
                                geotag = geotag,
                                link = link
                            };
                            db.Channels.Add(channelModel);
                            ChannelList.Add(new channelVM(channelModel));
                        }
                        else
                        {
                            found.link = link;
                            found.geotag = geotag;                            
                        }
                        db.SaveChanges();
                    }

                    //updateList();


                    SubContent = null;
                };
            });

            deleteCmd = ReactiveCommand.Create(() => {

                if (SelectedChannel == null)
                    return;               

                using (var db = new DataBaseContext())
                {
                    var found_db = db.Channels.FirstOrDefault(c => c.link.Equals(SelectedChannel.link));
                    if (found_db != null)
                    {
                        db.Channels.Remove(found_db);
                        db.SaveChanges();
                    }
                }
                var found_list = ChannelList.FirstOrDefault(c => c.link.Equals(SelectedChannel.link));    
                
                ChannelList.Remove(found_list);
            });

            //subscribeCmd = ReactiveCommand.CreateFromTask(async () => {
            //    if (SelectedChannel != null)
            //    {
            //        var link = SelectedChannel.link;
            //        if (SubscribeAllRequestEvent != null)
            //            await Task.Run(() => SubscribeAllRequestEvent(link));
            //    }
            //});

            //unsubscribeCmd = ReactiveCommand.CreateFromTask(async () =>
            //{
            //    if (SelectedChannel != null)
            //    {
            //        //UnsubscribeAllRequestEvent?.Invoke((long)SelectedChannel.tg_id);
            //        EventAggregator.getInstance().Publish((BaseEventMessage)(new ChannelUnsubscibeEventMessage(SelectedChannel.tg_id)));
            //    }
            //});
        }

        #region private
        async Task updateList()
        {
            await Task.Run(() => {
                ChannelList.Clear();

                List<ChannelModel> channels;

                using (var db = new DataBaseContext())
                {
                    channels = db.Channels.ToList();
                }
    
                foreach (var channel in channels)
                {
                    ChannelList.Add(new channelVM(channel));
                }
            });
        }
        #endregion

        #region public
        long tg_id_prev;
        public void updateChannelInfo(string link, long tg_id, string name)
        {
            if (tg_id == tg_id_prev)
                return;

            tg_id_prev = tg_id;

            using (var db = new DataBaseContext())
            {
                var found = db.Channels.FirstOrDefault(c => c.link.Contains(link));
                if (found != null)
                {
                    found.tg_id = tg_id;
                    found.name = name;
                }
                db.SaveChanges();
            }
            updateList();
        }

        public void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case ChannelListUpdateRequestEventMessage updateMessage:
                    updateChannelInfo(updateMessage.link, updateMessage.channel_id, updateMessage.name);
                    break;              
            }
        }
        #endregion

        #region events
        public event Action<string> SubscribeAllRequestEvent;
        public event Action<long> UnsubscribeAllRequestEvent;
        #endregion
    }
}
