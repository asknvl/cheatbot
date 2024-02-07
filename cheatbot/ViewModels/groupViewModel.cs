using asknvl;
using Avalonia.Diagnostics;
using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class groupViewModel : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        CancellationTokenSource cts;        
        #endregion

        #region properties
        public int Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<dropVM> GroupDropList { get; } = new();
        public ObservableCollection<channelSubsVM> ActionChannelsList { get; } = new();       
        public ObservableCollection<channelSubsVM> SelectedChannels { get; } = new();

        channelSubsVM selectedActionChannel;
        public channelSubsVM SelectedActionChannel
        {
            get => selectedActionChannel;
            set => this.RaiseAndSetIfChanged(ref selectedActionChannel, value); 
        }

        bool isStopVisible;
        public bool IsStopVisible
        {
            get => isStopVisible;
            set => this.RaiseAndSetIfChanged(ref isStopVisible, value); 
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addOneCmd { get; }
        public ReactiveCommand<Unit, Unit> removeOneCmd { get; }
        public ReactiveCommand<Unit, Unit> addAllCmd { get; }
        public ReactiveCommand<Unit, Unit> removeAllCmd { get; }
        public ReactiveCommand<Unit, Unit> subscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> unsubscribeCmd { get; }
        public ReactiveCommand<Unit, Unit> refreshCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        #endregion

        public groupViewModel(GroupModel model)
        {
            Id = model.id;
            Name = model.name;

            EventAggregator.getInstance().Subscribe(this);

            #region commands
            addOneCmd = ReactiveCommand.Create(() => {
                foreach (var item in SelectedChannels)
                {
                    if (!ActionChannelsList.Contains(item))
                        ActionChannelsList.Add(item);
                }
            });
            removeOneCmd = ReactiveCommand.Create(() => {
                if (ActionChannelsList.Contains(SelectedActionChannel))
                    ActionChannelsList.Remove(SelectedActionChannel);
            });

            removeAllCmd = ReactiveCommand.Create(() => {
                ActionChannelsList.Clear();
            });

            subscribeCmd = ReactiveCommand.CreateFromTask(async () => {

                if (cts != null)
                    return;

                var chModels = getChanelModels();

                List<Task> tasks = new List<Task>();

                cts = new CancellationTokenSource();

                IsStopVisible = true;

                foreach (var item in GroupDropList)
                {
                    var task = Task.Run(async () => {
                        await item.drop.Subscribe(chModels, cts);                                            
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks.ToArray());

                IsStopVisible = false;

                cts = null!;
            });

            unsubscribeCmd = ReactiveCommand.CreateFromTask(async () => {

                if (cts != null)
                    return;

                var chModels = getChanelModels();

                List<Task> tasks = new List<Task>();

                cts = new CancellationTokenSource();

                IsStopVisible = true;

                foreach (var item in GroupDropList)
                {
                    var task = Task.Run(async () => {
                        await item.drop.Unsubscribe(chModels, cts);
                    });

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks.ToArray());

                IsStopVisible = false;

                cts = null!;
            });

            stopCmd = ReactiveCommand.Create(() => { 
                cts?.Cancel();
            });
            #endregion
        }

        #region helpers
        List<ChannelModel> getChanelModels()
        {
            List<ChannelModel> models = new();
            foreach (var item in ActionChannelsList)
            {
                var model = new ChannelModel()
                {
                    link = item.Link,
                    tg_id = item.TG_id
                };
                models.Add(model);
            }
            return models;
        }
        #endregion

        #region private
        async Task updateViewedDrops(ObservableCollection<dropVM> dropList)
        {
            await Task.Run(() =>
            {
                GroupDropList.Clear();
                
                var viewedDrops = dropList.Where(d => d.group_id == Id);
                foreach (var drop in viewedDrops)
                {
                    GroupDropList.Add(drop);
                }                
            });
        }
        #endregion

        #region public    
        public async void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case DropListUpdatedEventMessage dluem:

                    if (dluem.group_id == Id)
                    {
                        await updateViewedDrops(dluem.drop_list);                                            
                    }
                    break;                
            }
        }
#endregion
    }
}
