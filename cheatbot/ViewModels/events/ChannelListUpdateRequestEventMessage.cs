using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelListUpdateRequestEventMessage : BaseEventMessage
    {
        public string link { get; }
        public long channel_id { get; }
        public string name { get; set; }

        public ChannelListUpdateRequestEventMessage() { }
        public ChannelListUpdateRequestEventMessage(string link, long channel_id, string name)
        {
            this.link = link;
            this.channel_id = channel_id;
            this.name = name;
        }
    }
}
