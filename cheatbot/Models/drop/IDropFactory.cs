using asknvl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.drop
{
    public interface IDropFactory
    {
        ITGUser Get(DropType type, string phone_number);
    }

    public enum DropType 
    {
        v0
    }
}
