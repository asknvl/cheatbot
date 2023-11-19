using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelUnsubscibeEventMessage : BaseEventMessage
    {
        public long tg_id { get; }
        public ChannelUnsubscibeEventMessage(long tg_id)
        {
            this.tg_id = tg_id;
        }
    }
}
