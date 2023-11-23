using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class AddDropEventMessage : BaseEventMessage
    {
        public string phone_number { get; }
        public string _2fa_password { get; }
        public int group_id { get; }

        public AddDropEventMessage(string phone_number, string _2fa_password, int group_id)
        {
            this.phone_number = phone_number;
            this._2fa_password = _2fa_password;
            this.group_id = group_id;
        }
    }
}
