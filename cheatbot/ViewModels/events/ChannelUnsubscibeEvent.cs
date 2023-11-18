using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class ChannelUnsubscibeEvent
    {
        public long tg_id { get; }
        public ChannelUnsubscibeEvent(long tg_id)
        {
            this.tg_id = tg_id;
        }
    }
}
