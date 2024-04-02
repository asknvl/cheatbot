using asknvl;
using cheatbot.Database.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.drop
{
    public interface IDropFactory
    {
        ITGUser Get(DropType type, string phone_number, string? old_2fa_password = null);        
    }

    public enum DropType 
    {
        v0,
        v1
    }
}
