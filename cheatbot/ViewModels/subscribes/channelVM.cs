using Avalonia.Controls.Selection;
using cheatbot.Database.models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.subscribes
{
    public class channelVM : ViewModelBase
    {
        #region vars
        static List<int> uniqSelection = new();
        List<dropVM> drops;
        List<GroupModel> group_models;
        #endregion

        #region properties
        string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        public long TG_id { get; set; }
        public string Link { get; set; }

        int totalActiveDrops;
        public int TotalActiveDrops
        {
            get => totalActiveDrops;
            set => this.RaiseAndSetIfChanged(ref totalActiveDrops, value);
        }
        public ObservableCollection<groupVM> Groups { get; set; } = new();
        //public ObservableCollection<groupVM> SelectedGroups { get; } = new();
        public SelectionModel<groupVM> Selection { get; }
        #endregion

        //public channelVM(string name, long )
        //{
            
        //}

        private void Selection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<groupVM> e)
        {
            Selection.SelectionChanged -= Selection_SelectionChanged;

            groupVM d,s = null;

            if (e.DeselectedItems.Count > 0)
            {
                d = e.DeselectedItems.First();
                if (uniqSelection.Contains(d.ID))
                    uniqSelection.Remove(d.ID); 
            }

            if (e.SelectedItems.Count > 0)
            {
                s = e.SelectedItems.First();
                if (!uniqSelection.Contains(s.ID))
                    uniqSelection.Add(s.ID);
                else
                    Selection.Deselect(e.SelectedIndexes.First());
            }

            Selection.SelectionChanged += Selection_SelectionChanged;
        }

        public channelVM(ChannelModel channel, List<GroupModel> group_models, List<dropVM> drops)
        {

            Selection = new SelectionModel<groupVM>();
            Selection.SingleSelect = false;
            Selection.SelectionChanged += Selection_SelectionChanged;

            Name = channel.geotag;
            TG_id = channel.tg_id;
            Link = channel.link;

            this.group_models = group_models;
            this.drops = drops;

            Update();
        }

        public void Update()
        {
            if (drops != null)
            {

                var channeled = drops.Where(d => d.drop.GetSubscribes().Contains(TG_id)).ToList();
                TotalActiveDrops = channeled.Count;

                foreach (var gm in group_models)
                {

                    var found = Groups.FirstOrDefault(g => g.ID == gm.id);
                    var grouped = channeled.Where(d => d.group_id == gm.id);

                    if (found == null)
                    {
                        var gr = new groupVM(gm.id, drops);
                        
                        gr.ActiveDropsInGroup = grouped.Count();
                        Groups.Add(gr);
                    }
                    else
                    {
                        found.ActiveDropsInGroup = grouped.Count();
                    }                  
                }
            }

        }
    }
}
