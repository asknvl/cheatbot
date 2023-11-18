using System;
using System.Threading.Tasks;

namespace asknvl
{
    public interface ITGUser
    {        
        public string api_id { get; set; }     
        public string api_hash { get; set; }                     
        string phone_number { get; set; } 
        long tg_id { get; set; }

        Task Start();
        Task Subscribe(string input);
        Task LeaveChannel(long id);
        void Stop();
        void SetVerifyCode(string code);

        event Action<ITGUser> VerificationCodeRequestEvent;
        event Action<ITGUser, bool> StartedEvent;
        event Action<ITGUser> StoppedEvent;
        event Action<string, long, string> ChannelAddedEvent;
    }
}
