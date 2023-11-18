using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace cheatbot.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {


        public ViewModelBase() { 

            EventAggregator.getInstance().Subscribe(this);  
        
        }
    }
}
