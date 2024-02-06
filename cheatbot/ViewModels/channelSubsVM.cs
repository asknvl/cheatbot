using JetBrains.Annotations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class channelSubsVM : ViewModelBase
    {
        #region properties
        public string Geotag { get; set; }        
        public string Link { get; set; }
        public ObservableCollection<channelGroupVM> Groups { get; } = new();

        int totalSubscribes;
        public int TotalSubscribes
        {
            get => totalSubscribes;
            set => this.RaiseAndSetIfChanged(ref totalSubscribes, value);   
        }
        public long TG_id { get; set; }
        #endregion

        public channelSubsVM()
        {

        }

        #region public
        public void UpdateSubsCounter(int delta)
        {
            if (TotalSubscribes + delta >= 0)
                TotalSubscribes += delta;
        }
        #endregion
    }
}
