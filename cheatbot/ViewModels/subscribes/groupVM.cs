using cheatbot.Database.models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.subscribes
{
    public class groupVM : ViewModelBase
    {
        #region vars
        List<dropVM> drops;
        #endregion

        #region properties
        int id;
        public int ID
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);    
        }

        int totalDropsInGroup;
        public int TotalDropsInGroup
        {
            get => totalDropsInGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref totalDropsInGroup, value);
                updateStatus();
            }
        }

        int activeDropsInGroup;
        public int ActiveDropsInGroup
        {
            get => activeDropsInGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref activeDropsInGroup, value);
                updateStatus();
            }
        }

        GroupStatus groupStatus;
        public GroupStatus GroupStatus
        {
            get => groupStatus;
            set => this.RaiseAndSetIfChanged(ref  groupStatus, value);  
        }
        #endregion

        public groupVM(int id, List<dropVM> drops)
        {
            ID = id;
            this.drops = drops;
        }

        #region helpers
        void updateStatus()
        {
            if (ActiveDropsInGroup == 0)
                GroupStatus = GroupStatus.ignore;
            else
            //if (ActiveDropsInGroup > 0 && (ActiveDropsInGroup < TotalDropsInGroup))
            if (ActiveDropsInGroup > 0 && (TotalDropsInGroup - ActiveDropsInGroup > 10))
                GroupStatus = GroupStatus.part;
            else
                GroupStatus = GroupStatus.full;
        }
        #endregion

        #region public
        public async Task Subscribe(channelVM channel, CancellationTokenSource cts)
        {
            var groupedDrops = drops.Where(d => d.group_id == ID).ToList();

            var model = new ChannelModel()
            {
                tg_id = channel.TG_id,
                link = channel.Link,
                geotag = channel.Name
            };

            try
            {
                foreach (var drop in groupedDrops)
                {
                    await drop.drop.Subscribe(new List<ChannelModel>() { model }, cts);
                }
            } catch (Exception ex)
            {

            }
        }

        public async Task Unsubscribe(channelVM channel, CancellationTokenSource cts)
        {
            var groupedDrops = drops.Where(d => d.group_id == ID).ToList();

            var model = new ChannelModel()
            {
                tg_id = channel.TG_id,
                link = channel.Link,
                geotag = channel.Name
            };

            try
            {
                foreach (var drop in groupedDrops)
                {
                    await drop.drop.Unsubscribe(new List<ChannelModel>() { model }, cts);
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }

    public enum GroupStatus
    {
        full,
        part,
        ignore
    }
}
