using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelMessageViewedEventMessage : BaseEventMessage
    {
        public long channel_id { get; }
        public uint counter { get; }
        public ChannelMessageViewedEventMessage(long channel_id, uint counter)
        {
            this.channel_id = channel_id;
            this.counter = counter; 
        }
    }
}
