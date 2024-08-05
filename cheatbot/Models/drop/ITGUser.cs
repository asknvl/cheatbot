using cheatbot.Database.models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TL;

namespace asknvl
{
    public interface ITGUser
    {        
        public string api_id { get; set; }     
        public string api_hash { get; set; }                     
        string phone_number { get; set; }
        public string _2fa_password { get; }
        long tg_id { get; set; }
        //bool is_active { get; set; }
        DropStatus status { get; set; }
        bool is_subscription_running { get; set; }  
        bool test_mode { get; set; }

        Task Start(string proxy);
        Task Subscribe(string input);
        Task Subscribe(List<ChannelModel> channels, CancellationTokenSource cts);
        Task Subscribe(string bot_username, CancellationTokenSource cts);
        List<long> GetSubscribes();
        Task Unsubscribe(long id);
        Task Unsubscribe(List<ChannelModel> channels, CancellationTokenSource cts);
        Task Change2FAPassword(string old_password, string new_password);
        Task Stop();
        void SetVerifyCode(string code);
        void ClearSession();

        event Action<ITGUser> VerificationCodeRequestEvent;        
        event Action<ITGUser, DropStatus> StatusChangedEvent;
        
        event Action<long, uint> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;
    }

    public enum DropStatus
    {
        stopped,
        active,        
        verification,
        revoked,
        banned,
        removed,
        subscription,
        idled
    }

    public class messageInfo
    {
        public long peer_id { get; set; }
        public int message_id { get; set; }
        public long grouped_id { get; set; }

        public messageInfo(UpdateNewMessage unm)
        {
            peer_id = unm.message.Peer.ID;
            message_id = unm.message.ID;
            grouped_id = ((Message)unm.message).grouped_id;
        }

        public messageInfo(long peer_id, int message_id)
        {
            this.peer_id = peer_id;
            this.message_id = message_id;
        }
    }

    public class pollInfo
    {

        public MessageMediaPoll? poll { get; set; }
        public Peer? peer { get; set; }
        public int id { get; set; }

        public pollInfo(MessageMediaPoll? poll, Peer? peer, int id)
        {
            this.poll = poll;
            this.peer = peer;
            this.id = id;
        }
    }
}
