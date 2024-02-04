using asknvl;
using cheatbot.Database.models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class dropSubVM : ViewModelBase
    {
        #region vars
        ITGUser drop;
        #endregion

        #region properties       
        public string phone_number { get; }        
        public int group_id { get; }
        DropStatus Status { get; }
        #endregion

        public dropSubVM(DropModel model)
        {
            phone_number = model.phone_number;
            group_id = model.group_id;  
        }

    }
}
