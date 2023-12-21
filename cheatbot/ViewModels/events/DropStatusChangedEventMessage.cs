using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class DropStatusChangedEventMessage : BaseEventMessage
    {
        public bool is_running { get; set; }
        public DropStatusChangedEventMessage(bool is_running) {
            this.is_running = is_running;
        }
    }
}
