using System;
using System.Threading.Tasks;

namespace asknvl
{
    public interface ITGUser
    {        
        public string api_id { get; set; }     
        public string api_hash { get; set; }                     
        string phone_number { get; set; }
        public string _2fa_password { get; }
        long tg_id { get; set; }
        bool is_active { get; set; }

        Task Start();
        Task Subscribe(string input);
        Task LeaveChannel(long id);
        Task Change2FAPassword(string old_password, string new_password);
        Task Stop();
        void SetVerifyCode(string code);

        event Action<ITGUser> VerificationCodeRequestEvent;
        event Action<ITGUser, bool> StartedEvent;
        event Action<ITGUser> StoppedEvent;
        event Action<string, long, string> ChannelAddedEvent;
        event Action<long, uint> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;
    }
}
