using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class GroupStoppedEventMessage : BaseEventMessage
    {
        public int group_id { get; set; }
        public GroupStoppedEventMessage(int group_id)
        {
            this.group_id = group_id;
        }
    }
}
