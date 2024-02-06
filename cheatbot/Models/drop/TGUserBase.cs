
using asknvl.logger;
using cheatbot.Database;
using cheatbot.Database.models;
using Starksoft.Net.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace asknvl
{
    public abstract class TGUserBase : ITGUser
    {
        #region vars
        protected Client user;
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;
        protected ILogger logger;

        protected Dictionary<long, ChatBase> chats = new();
        protected Dictionary<long, User> users = new();

        //protected Messages_Chats chats;
        protected List<Messages_ChatFull> fullChats = new();
        protected Messages_Dialogs dialogs;
        string dir = Path.Combine("C:", "userpool");

        protected List<long> acceptedIds = new();
        Random random = new Random();
        #endregion

        #region properties        
        public string api_id { get; set; }
        public string api_hash { get; set; }
        public string phone_number { get; set; }
        public long tg_id { get; set; }
        public string? username { get; set; }
        public string _2fa_password { get; }
        //public bool is_active { get; set; }
        public DropStatus status { get; set; }
        public bool test_mode { get; set; }
        public bool is_subscription_running { get; set; }
        #endregion

        public TGUserBase(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger)
        {
            this.api_id = api_id;
            this.api_hash = api_hash;
            this.phone_number = phone_number;
            this.logger = logger;
            this._2fa_password = _2fa_password;
            this.status = DropStatus.stopped;
        }

        #region protected
        protected string Config(string what)
        {


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            switch (what)
            {
                case "api_id": return api_id;
                case "api_hash": return api_hash;
                case "session_pathname": return $"{dir}/{phone_number}.session";
                case "phone_number": return phone_number;
                case "verification_code":
                    setStatus(DropStatus.verification);
                    VerificationCodeRequestEvent?.Invoke(this);
                    verifyCodeReady.Reset();
                    verifyCodeReady.Wait();
                    return verifyCode;
                //case "first_name": return "Stevie";
                //case "last_name": return "Voughan";
                case "password": return _2fa_password;
                default: return null;
            }
        }

        abstract protected Task processUpdate(TL.Update update);
        #endregion

        #region private
        private async Task OnUpdate(UpdatesBase updates)
        {
            updates.CollectUsersChats(users, chats);

            if (updates is UpdateShortMessage usm && !users.ContainsKey(usm.user_id))
                (await user.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(users, chats);
            else if (updates is UpdateShortChatMessage uscm && (!users.ContainsKey(uscm.from_id) || !chats.ContainsKey(uscm.chat_id)))
                (await user.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(users, chats);

            //if (arg is not UpdatesBase updates)
            //    return;

            foreach (var update in updates.UpdateList)
            {
                await processUpdate(update);
            }
        }
        #endregion

        #region public
        public virtual Task Start(string proxy)
        {
            if (status == DropStatus.active)
                return Task.CompletedTask;

            string[] proxy_cred = null!;
            int port = 0;

            if (!string.IsNullOrEmpty(proxy))
            {
                try
                {
                    proxy_cred = proxy.Split(':');
                    if (proxy_cred.Length < 4)
                        throw new ArgumentException("Неправильный формат адреса прокси");
                    port = int.Parse(proxy_cred[1]);

                }
                catch (Exception ex)
                {
                    logger.err(phone_number, ex.Message);
                    throw;
                }
            }

            User usr = null!;

            return Task.Run(async () =>
            {
                try
                {
                    user = new Client(Config);

                    if (proxy_cred != null)
                    {
                        user.TcpHandler = async (address, port) =>
                        {
                            var proxy = new Socks5ProxyClient(
                                                        proxy_cred[0],
                                                        int.Parse(proxy_cred[1]),
                                                        proxy_cred[2],
                                                        proxy_cred[3]);
                            return proxy.CreateConnection(address, port);
                        };
                    }

                    logger.inf(phone_number, $"Starting...");

                    usr = await user.LoginUserIfNeeded();
                    username = usr.username;
                    tg_id = usr.ID;

                    //chats = await user.Messages_GetAllChats();
                    dialogs = await user.Messages_GetAllDialogs();
                    dialogs.CollectUsersChats(users, chats);

                    using (var db = new DataBaseContext())
                    {
                        acceptedIds = db.Channels.Select(c => c.tg_id).ToList();
                    }

                    user.OnUpdate -= OnUpdate;
                    user.OnUpdate += OnUpdate;

                    setStatus(DropStatus.active);

                }
                catch (RpcException ex)
                {
                    processRpcException(ex);
                }
                catch (Exception ex)
                {
                    logger.err(phone_number, $"Starting fail: {ex.Message}");
                    await Stop();
                }

            }).ContinueWith(async t =>
            {
                //StartedEvent?.Invoke(this, is_active);

                if (status == DropStatus.active)
                {
                    await user.Account_UpdateStatus(false);
                    logger.inf(phone_number, $"Started OK");
                }
            });
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }

        public void ClearSession()
        {
            if (true/*status == DropStatus.revoked || status == DropStatus.banned || status == DropStatus.verification*/)
            {
                try
                {
                    var sp = Path.Combine(dir, $"{phone_number}.session");
                    if (File.Exists(sp))
                    {
                        user?.Dispose();
                        File.Delete(sp);
                    }
                }
                catch (Exception ex)
                {
                    logger.err("ClearSession:", $"{ex.Message}");
                }
            }
        }

        public async Task Subscribe(string input)
        {
            string hash = "";
            string[] splt = input.Split("/");
            input = splt.Last();

            if (input.Contains("+"))
            {
                hash = input.Replace("+", "");

                ChatInviteBase cci = null;
                UpdatesBase ici = null;

                try
                {
                    cci = await user.Messages_CheckChatInvite(hash);
                }
                catch (Exception ex)
                {
                    logger.err(phone_number, ex.Message);
                }
                switch (cci)
                {
                    case ChatInvite invite:
                    case ChatInvitePeek peek:
                        ici = await user.Messages_ImportChatInvite(hash);
                        ChannelAddedEvent?.Invoke(input, ici.Chats.First().Key, ici.Chats.First().Value.Title);
                        logger.inf(phone_number, $"JoinedChannel: {ici.Chats.First().Value.Title} OK");
                        break;
                    case ChatInviteAlready already:
                        ChannelAddedEvent?.Invoke(input, already.chat.ID, already.chat.Title);
                        logger.inf(phone_number, $"JoinedChannel: {already.chat.Title} OK");
                        break;
                }

            }
            else
            {
                hash = input.Replace("@", "");
                var resolved = await user.Contacts_ResolveUsername(hash); // without the @
                if (resolved.Chat is Channel channel)
                {
                    await user.Channels_JoinChannel(channel);
                    ChannelAddedEvent?.Invoke(input, channel.ID, channel.Title);
                    logger.inf(phone_number, $"JoinedChannel: {channel.Title} OK");
                }
            }

            //chats = await user.Messages_GetAllChats();
        }

        public async Task Subscribe(List<ChannelModel> channels, CancellationTokenSource cts)
        {
            if (status != DropStatus.active)
                return;

            setStatus(DropStatus.subscription);

            //chats = await user.Messages_GetAllChats();

            List<ChannelModel> delta = new();

            foreach (var channel in channels)
            {
                var found = chats.ContainsKey(channel.tg_id);
                if (!found)
                    delta.Add(channel);
            }

            var randomDelta = delta.OrderBy(item => random.Next()).ToList();

            try
            {
                foreach (var channel in randomDelta)
                {
                    cts.Token.ThrowIfCancellationRequested();
#if DEBUG_FAST
                    await Task.Delay(random.Next(5, 20) * 1 * 1000);
#else
                    await Task.Delay(random.Next(5, 20) * 60 * 1000);
#endif
                    await Subscribe(channel.link);

                }
            }
            catch (OperationCanceledException ex)
            {

            }
            catch (Exception ex)
            {
                logger.err(phone_number, $"Subscribe: {ex.Message}");
            } finally
            {
                //chats = await user.Messages_GetAllChats();

                using (var db = new DataBaseContext())
                {
                    acceptedIds = db.Channels.Select(c => c.tg_id).ToList();
                }

                setStatus(DropStatus.active);
            }
        }

        public List<long> GetSubscribes()
        {
            List<long> res = new();

            if (status == DropStatus.active || status == DropStatus.subscription)
                return chats.Keys.ToList();

            return res;
        }

        public async Task Unsubscribe(long channel_tg_id)
        {
            if (user == null)
                return;

            try
            {
                var chats = await user.Messages_GetAllChats();

                if (!chats.chats.ContainsKey(channel_tg_id))
                {
                    ChannelLeftEvent?.Invoke(channel_tg_id);
                    return;
                }

                var chat = chats.chats[channel_tg_id];
                await user.LeaveChat(chat);
                ChannelLeftEvent?.Invoke(channel_tg_id);
                logger.inf(phone_number, $"LeaveChannel: {chat.Title} OK");

            }
            catch (Exception ex)
            {
                logger.err(phone_number, $"LeaveChannel: {ex.Message}");
            }
        }

        public async Task Unsubscribe(List<ChannelModel> channels, CancellationTokenSource cts)
        {
            if (status != DropStatus.active)
                return;

            setStatus(DropStatus.subscription);

            try
            {
                //chats = await user.Messages_GetAllChats();

                var randomChannels = channels.OrderBy(item => random.Next()).ToList();

                foreach (var channel in randomChannels)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    var found = chats.ContainsKey(channel.tg_id);
                    if (found)
                    {
                        try
                        {
#if DEBUG_FAST
                            await Task.Delay(random.Next(5, 20) * 1 * 1000);
#else
                            await Task.Delay(random.Next(5, 20) * 60 * 1000);
#endif
                            var chat = chats[channel.tg_id];
                            await user.LeaveChat(chat);
                            ChannelLeftEvent?.Invoke(channel.tg_id);
                            logger.inf(phone_number, $"LeaveChannel: {chat.Title} OK");
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                logger.err(phone_number, $"Unsubscribe: {ex.Message}");
            } finally
            {
                //chats = await user.Messages_GetAllChats();
                setStatus(DropStatus.active);
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

            }
            catch (Exception ex)
            {
                logger.err(phone_number, $"Change2FAPassword: {ex.Message}");
            }
        }

        public virtual async Task Stop()
        {
            await Task.Run(() =>
            {
                user?.Dispose();
                verifyCodeReady.Set();
                //StoppedEvent?.Invoke(this);                
                setStatus(DropStatus.stopped);
            });
        }
        #endregion

        #region protected
        protected void SendChannelMessageViewedEvent(long channel_id, uint counter)
        {
            ChannelMessageViewedEvent?.Invoke(channel_id, counter);
        }

        protected void setStatus(DropStatus _status)
        {

            if (status != _status)
            {
                status = _status;
                StatusChangedEvent?.Invoke(this, status);
                logger.dbg(phone_number, $"{status}");
            }
        }

        protected void processRpcException(RpcException ex)
        {
            switch (ex.Message)
            {
                case "PHONE_NUMBER_BANNED":
                    setStatus(DropStatus.banned);
                    user.Dispose();
                    break;

                case "SESSION_REVOKED":
                case "AUTH_KEY_UNREGISTERED":
                    user.Dispose();
                    setStatus(DropStatus.revoked);
                    break;

            }
        }
        #endregion

        #region events
        public event Action<ITGUser> VerificationCodeRequestEvent;
        public event Action<string, long, string> ChannelAddedEvent;
        public event Action<long> ChannelLeftEvent;

        public event Action<long, uint> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;
        public event Action<ITGUser, DropStatus> StatusChangedEvent;
        #endregion
    }
}
