using cheatbot.Database.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.server
{
    public interface IChannelsProvider
    {
        Task<List<ChannelModel>> GetChannels();
    }
}
