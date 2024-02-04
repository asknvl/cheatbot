using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class autoSubscribesVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {

        #region properties
        public ObservableCollection<dropVM> DropList { get; } = new();
        public ObservableCollection<dropVM> ViewedDropList { get; } = new();

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
                updateViewedDrops();
            }
        }
        #endregion

        public autoSubscribesVM(ObservableCollection<dropVM> dropVMs)
        {
            DropList = dropVMs;
            loadGroups();
        }

        #region helpers                
        async Task updateViewedDrops()
        {
            await Task.Run(() =>
            {
                ViewedDropList.Clear();

                if (SelectedGroup != null)
                {
                    var viewedDrops = DropList.Where(d => d.group_id == SelectedGroup.id);
                    foreach (var drop in viewedDrops)
                    {
                        ViewedDropList.Add(drop);
                    }
                }
            });

        }
        async Task loadGroups()
        {
            await Task.Run(() =>
            {
                using (var db = new DataBaseContext())
                {
                    Groups = db.Groups.ToList();
                    if (Groups.Count > 0)
                        SelectedGroup = Groups[0];
                }
            });
        }
        #endregion
        public void OnEvent(BaseEventMessage message)
        {
            //throw new NotImplementedException();
        }
    }
}
