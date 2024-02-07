using asknvl;
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
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class autoSubscribesVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {

        #region vars
        List<dropVM> dropList;
        #endregion

        #region properties        
        public ObservableCollection<groupViewModel> Groups { get; } = new();
      
        groupViewModel selectedGroup;
        public groupViewModel SelectedGroup
        {
            get => selectedGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedGroup, value);                
            }
        }

        public ObservableCollection<channelSubsVM> ChannelsList { get; } = new();
        #endregion

        #region commands        
        public ReactiveCommand<Unit, Unit> refreshCmd { get; }
        #endregion

        public autoSubscribesVM()
        {
            EventAggregator.getInstance().Subscribe(this);

            //this.dropList = dropList;
            loadGroups();
            //loadChannels();

            #region commands
            refreshCmd = ReactiveCommand.CreateFromTask(async () => {
                await loadChannels();
            });
            #endregion

        }

        #region helpers                
        async void loadGroups()
        {
            await Task.Run(() =>
            {
                using (var db = new DataBaseContext())
                {
                    var models = db.Groups.ToList();
                    foreach (var model in models)
                    {
                        Groups.Add(new groupViewModel(model));
                    }

                    if (Groups.Count > 0)
                        SelectedGroup = Groups[0];
                }
            });
        }

        
        #endregion
        public void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {   
                case DropListUpdatedEventMessage dluem:
                    dropList = dluem.drop_list.ToList();             
                    break;
            }
        }

        public async Task loadChannels()
        {

            await Task.Run(() =>
            {

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ChannelsList.Clear();
                });

                using (var db = new DataBaseContext())
                {
                    var channels = db.Channels.ToList();
                    var groups = db.Groups.ToList();

                    foreach (var channel in channels)
                    {

                        int channeled_count = 0;
                        List<dropVM> channeled = new();

                        if (dropList != null)
                        {
                            channeled = dropList.Where(d => d.drop.GetSubscribes().Contains(channel.tg_id)).ToList();
                            channeled_count = channeled.Count;
                        }



                        var chSubs = new channelSubsVM();
                        chSubs.Geotag = channel.geotag;
                        chSubs.TG_id = channel.tg_id;
                        chSubs.Link = channel.link;

                        chSubs.TotalSubscribes = channeled_count;

                        foreach (var g in groups)
                        {

                            var chGrp = new channelGroupVM();
                            //var grouped = db.Drops.Where(d => d.group_id == g.id);
                            var grouped = channeled.Where(d => d.group_id == g.id);
                            //var selected = channeled.Where(i => grouped.Any(j => j.phone_number.Equals(i.phone_number)));



                            chSubs.Groups.Add(chGrp);
                            //chGrp.ID = selected.Count();
                            chGrp.ID = grouped.Count();
                        }


                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ChannelsList.Add(chSubs);
                        });

                        //ChannelsList.Add(new channelSubsVM()
                        //{
                        //    Geotag = channel.geotag,
                        //    TG_id = channel.tg_id,
                        //    Link = channel.link,
                        //    TotalSubscribes = count,                                
                        //});

                    }
                }

            });
        }
    }
}
