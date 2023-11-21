
using asknvl.logger;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace asknvl
{
    public abstract class TGUserBase : ITGUser
    {
        #region vars
        protected Client user;
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;
        protected ILogger logger;
        Messages_Chats chats;        
        #endregion

        #region properties        
        public string api_id { get; set; }        
        public string api_hash { get; set; }        
        public string phone_number { get; set; }    
        public long tg_id { get; set; }         
        public string? username { get; set; }
        public string _2fa_password { get; }
        #endregion

        public TGUserBase(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger)
        {            
            this.api_id = api_id;
            this.api_hash = api_hash;
            this.phone_number = phone_number;            
            this.logger = logger;

            this._2fa_password = _2fa_password;
        }

        #region protected
        protected string Config(string what)
        {

            //string dir = Path.Combine(Directory.GetCurrentDirectory(), "userpool");
            string dir = Path.Combine("C:", "userpool");


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            switch (what)
            {
                case "api_id": return api_id;
                case "api_hash": return api_hash;
                case "session_pathname": return $"{dir}/{phone_number}.session";
                case "phone_number": return phone_number;
                case "verification_code":
                    VerificationCodeRequestEvent?.Invoke(this);
                    verifyCodeReady.Reset();
                    verifyCodeReady.Wait();
                    return verifyCode;
                case "first_name": return "Stevie";  
                case "last_name": return "Voughan";  
                case "password": return _2fa_password;  
                default: return null;
            }
        }

        abstract protected void processUpdate(Update update);
        #endregion

        #region private
        private async Task OnUpdate(TL.IObject arg)
        {
            if (arg is not UpdatesBase updates)
                return;

            foreach (var update in updates.UpdateList)
            {
                processUpdate(update);
            }
        }
        #endregion

        #region public
        public virtual Task Start()
        {
            //logger = new Logger("USR", "chains", $"{chain}_{phone_number}");
            logger.inf(phone_number, $"Starting...");

            User usr = null;
            bool res = false;

            return Task.Run(async () =>
            {
                try
                {
                    user = new Client(Config);                    
                    usr = await user.LoginUserIfNeeded();
                    username = usr.username;
                    tg_id = usr.ID;

                    chats = await user.Messages_GetAllChats();

                    user.OnUpdate -= OnUpdate;
                    user.OnUpdate += OnUpdate;
                    res = true;
                } catch (Exception ex)
                {
                    logger.err(phone_number, $"Starting fail: {ex.Message}");
                }

            }).ContinueWith(t =>
            {
                StartedEvent?.Invoke(this, res);
                logger.inf(phone_number, $"Started OK");
            });
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }

        public async Task Subscribe(string input)
        {
            string hash = "";
            string[] splt = input.Split("/");
            input = splt.Last();

            if (input.Contains("+"))
            {
                hash = input.Replace("+", "");
                var cci = await user.Messages_CheckChatInvite(hash);
                var ici = await user.Messages_ImportChatInvite(hash);
                ChannelAddedEvent?.Invoke(input, ici.Chats.First().Key, ici.Chats.First().Value.Title);
                
            }
            else
            {
                hash = input.Replace("@", "");
                var resolved = await user.Contacts_ResolveUsername(hash); // without the @
                if (resolved.Chat is Channel channel)
                {
                    await user.Channels_JoinChannel(channel);
                }
            }

            chats = await user.Messages_GetAllChats();
        }

        public async Task LeaveChannel(long id)
        {
            try
            {
                var chats = await user.Messages_GetAllChats();
                var chat = chats.chats[id];
                await user.LeaveChat(chat);
                logger.inf(phone_number, $"LeaveChannel: {chat.Title} OK");

            } catch (Exception ex)
            {
                logger.err(phone_number, $"LeaveChannel: {ex.Message}");
            }
        }
        
        public async Task Change2FAPassword(string old_password, string new_password)
        {
            try
            {
                var accountPwd = await user.Account_GetPassword();
                var password = accountPwd.current_algo == null ? null : await WTelegram.Client.InputCheckPassword(accountPwd, old_password);
                accountPwd.current_algo = null; // makes InputCheckPassword generate a new password
                var new_password_hash = new_password == null ? null : await WTelegram.Client.InputCheckPassword(accountPwd, new_password);
                await user.Account_UpdatePasswordSettings(password, new Account_PasswordInputSettings
                {
                    flags = Account_PasswordInputSettings.Flags.has_new_algo,
                    new_password_hash = new_password_hash?.A,
                    new_algo = accountPwd.new_algo
                    //hint = "new password hint",
                });

                _2FAPasswordChanged.Invoke(new_password);

            } catch (Exception ex)
            {
                logger.err(phone_number, $"Change2FAPassword: {ex.Message}");
            }
        }

        public virtual void Stop()
        {            
            user?.Dispose();
            StoppedEvent?.Invoke(this);
            logger.inf(phone_number, $"Stopped");
        }
        #endregion

        #region protected
        protected void SendChannelMessageViewedEvent(long channel_id)
        {
            ChannelMessageViewedEvent?.Invoke(channel_id);
        }
        #endregion

        #region events
        public event Action<ITGUser> VerificationCodeRequestEvent;
        public event Action<ITGUser, bool> StartedEvent;
        public event Action<ITGUser> StoppedEvent;
        public event Action<string, long, string> ChannelAddedEvent;
        public event Action<long> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;  
        #endregion
    }
}
