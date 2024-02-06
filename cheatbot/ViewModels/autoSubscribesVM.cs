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

        #region properties
        //public dropListVM dropList;
        public ObservableCollection<groupViewModel> Groups { get; } = new();
      
        groupViewModel selectedGroup;
        public groupViewModel SelectedGroup
        {
            get => selectedGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedGroup, value);
                //updateViewedDrops();
            }
        }
        #endregion

        #region commands        
        #endregion

        public autoSubscribesVM(dropListVM dropList)
        {
            EventAggregator.getInstance().Subscribe(this);

            //this.dropList = dropList;
            loadGroups();
            //loadChannels();

            #region commands
           
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
                case DropStatusChangedEventMessage dscem:

                    //int delta = (dscem.status == DropStatus.active) ? 1 : -1;

                    //foreach (var channel in ChannelsList)
                    //{
                    //    var found = dscem.subscribes.FirstOrDefault(s => s == channel.TG_id);
                    //    if (found != null)                        
                    //        channel.UpdateSubsCounter(delta);
                    //}

                    break;
            }
        }
    }
}
