using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public interface IEventSubscriber<TMessage> where TMessage : class
    {
        void OnEvent(TMessage message);
    }
}
