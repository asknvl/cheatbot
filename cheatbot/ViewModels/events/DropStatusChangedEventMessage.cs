using asknvl;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class DropStatusChangedEventMessage : BaseEventMessage
    {
        //public bool is_running { get; set; }
        public int group_id { get; set; }
        public int id { get; set; }
        public string phone_number { get; set; }
        public DropStatus status { get; set; }        

        public DropStatusChangedEventMessage(int _group_id, int _id, string _phone_number, DropStatus _status) {
            group_id = _group_id;
            id = _id;
            phone_number = _phone_number;
            status = _status;
        }
    }
}
