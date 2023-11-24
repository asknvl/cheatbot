using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelUnsubscribeRequestEventMessage : BaseEventMessage
    {
        public int group_id { get; set; }        
        public long tg_id { get; set; }       

        public ChannelUnsubscribeRequestEventMessage(int group_id, long tg_id)
        {
            this.group_id = group_id;
            this.tg_id = tg_id;
        }
    }
}
