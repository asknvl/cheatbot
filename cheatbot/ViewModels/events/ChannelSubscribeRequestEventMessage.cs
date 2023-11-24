using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelSubscribeRequestEventMessage : BaseEventMessage
    {
        public int group_id { get; set; }
        public int channel_id { get; set; }
        public string link { get; set; }

        public ChannelSubscribeRequestEventMessage(int group_id, string link)
        {
            this.group_id = group_id;
            this.link = link;
        }
    }
}
