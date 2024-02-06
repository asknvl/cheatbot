using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class DropListUpdatedEventMessage : BaseEventMessage
    {
        public int group_id { get; set; }
        public ObservableCollection<dropVM> drop_list { get; set; }

        public DropListUpdatedEventMessage(int group_id, ObservableCollection<dropVM> drop_list)
        {
            this.group_id = group_id;
            this.drop_list = drop_list;
        }       

    }
}
