using asknvl;
using asknvl.logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace cheatbot.Models.drop
{
    public class Drop_v1 : TGUserBase
    {
        public Drop_v1(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, _2fa_password, logger)
        {
        }

        protected override Task processUpdate(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
