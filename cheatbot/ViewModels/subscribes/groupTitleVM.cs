using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.subscribes
{
    public class groupTitleVM : ViewModelBase
    {
        string id;
        public string ID
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        int totalDropsInGroup;
        public int TotalDropsInGroup
        {
            get => totalDropsInGroup;
            set => this.RaiseAndSetIfChanged(ref totalDropsInGroup, value); 
        }
    }
}
