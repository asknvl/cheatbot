using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class DropRemoveRequestMessage : BaseEventMessage
    {
        public int id { get; set; }
        public string phone_number { get; set; }
        public string path { get; set; }

        public DropRemoveRequestMessage(int id, string phone_number, string path)
        {
            this.id = id;
            this.phone_number = phone_number;
            this.path = path;
        }
    }
}
