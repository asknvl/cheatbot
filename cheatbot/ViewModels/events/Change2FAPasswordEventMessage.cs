using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class Change2FAPasswordAllEventMessage : BaseEventMessage
    {        
        public string old_password { get; }
        public string new_password { get; }
        public Change2FAPasswordAllEventMessage(string old_password, string new_password)
        {
            this.old_password = old_password;
            this.new_password = new_password;
        }
    }

    public class Change2FAPasswordOneEventMessage : BaseEventMessage
    {
        public string phone_number { get; set; }
        public string old_password { get; }
        public string new_password { get; }
        public Change2FAPasswordOneEventMessage(string phone_number, string old_password, string new_password)
        {
            this.phone_number = phone_number;
            this.old_password = old_password;
            this.new_password = new_password;
        }
    }
}
