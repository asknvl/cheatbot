using asknvl;
using asknvl.logger;
using cheatbot.Database;
using cheatbot.Database.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.drop
{
    public class DropFactory : IDropFactory
    {
        #region vars
        string api_id = string.Empty;
        string api_hash = string.Empty;
        ILogger logger;
        #endregion

        public DropFactory(ILogger logger) {
            using (var db = new DataBaseContext())
            {
                var settings = db.ApiSettings.FirstOrDefault(s => s.group == 0);
                api_id = settings.api_id;
                api_hash = settings.api_hash;

                this.logger = logger;
            }
        }

        public ITGUser Get(DropType type, string phone_number, string old_2fa_password)
        {            
            switch (type)
            {
                case DropType.v0:
                    return new Drop_v0(api_id, api_hash, phone_number, old_2fa_password, logger);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
