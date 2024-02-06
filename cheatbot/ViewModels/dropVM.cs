using asknvl;
using asknvl.logger;
using Avalonia.Controls;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.Models.drop;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class dropVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        IDropFactory dropFactory;        
        ILogger logger;
        #endregion

        #region properties
        public ITGUser drop { get; }

        int _id;
        public int id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        string _phone_number;
        public string phone_number
        {
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string __2fa_password;
        public string _2fa_password
        {
            get => __2fa_password;
            set => this.RaiseAndSetIfChanged(ref __2fa_password, value);
        }

        public int group_id { get; }

        string _code;
        public string code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }

        bool allowStart;
        public bool AllowStart
        {
            get => allowStart;
            set => this.RaiseAndSetIfChanged(ref allowStart, value);
        }

        //bool isRunning;
        //public bool IsRunning
        //{
        //    get => isRunning;
        //    set => this.RaiseAndSetIfChanged(ref isRunning, value);
        //}
        DropStatus status;
        public DropStatus Status
        {
            get => status;
            set
            {

                switch (value)
                {
                    case DropStatus.revoked:
                    case DropStatus.verification:
                    case DropStatus.banned:
                        AllowRemove = true;
                        break;
                    default:
                        AllowRemove = false;
                        break;
                }

                this.RaiseAndSetIfChanged(ref status, value);
            }
        }

        bool needVerification;
        public bool NeedVerification
        {
            get => needVerification;
            set => this.RaiseAndSetIfChanged(ref needVerification, value);
        }

        bool allowRemove;
        public bool AllowRemove
        {
            get => allowRemove;
            set => this.RaiseAndSetIfChanged(ref allowRemove, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> verifyCmd { get; }
        public ReactiveCommand<Unit, Unit> openTgCmd { get; }
        public ReactiveCommand<Unit, Unit> clearSessionCmd { get; }
        public ReactiveCommand<Unit, Unit> removeCmd { get; }
        #endregion

        public dropVM(DropModel model, ILogger logger)
        {

            this.logger = logger;

            phone_number = model.phone_number;
            _2fa_password = model._2fa_password;
            group_id = model.group_id;
            id = model.id;

            dropFactory = new DropFactory(logger);

            drop = dropFactory.Get(DropType.v0, phone_number, _2fa_password);
            drop.ChannelAddedEvent += Drop_ChannelAddedEvent;
            drop.ChannelMessageViewedEvent += Drop_ChannelMessageViewedEvent;
            drop.ChannelLeftEvent += Drop_ChannelLeftEvent;
            drop._2FAPasswordChanged += Drop__2FAPasswordChanged;


            drop.VerificationCodeRequestEvent += Drop_VerificationCodeRequestEvent;

            //drop.StartedEvent += Drop_StartedEvent;
            //drop.StoppedEvent += Drop_StoppedEvent;

            drop.StatusChangedEvent -= Drop_StatusChangedEvent;
            drop.StatusChangedEvent += Drop_StatusChangedEvent;

            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                //if (group_id == 8)
                //    drop.test_mode = true;
                string? proxy = null;
                using (var db = new DataBaseContext())
                {
                    var found = db.AppSettings.FirstOrDefault();
                    if (found != null)
                    {
                        proxy = found.ProxyString;
                    }
                }
                await drop.Start(proxy);
            });
            stopCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await drop.Stop();
            });
            verifyCmd = ReactiveCommand.Create(() =>
            {
                drop.SetVerifyCode(code);
            });
            openTgCmd = ReactiveCommand.Create(() => {

                string root = "";
                using (var db = new DataBaseContext())
                {
                    var settings = db.AppSettings.FirstOrDefault();
                    root = settings.RootPath;
                }

                if (!string.IsNullOrEmpty(root))
                {
                    var tg_path = Path.Combine(root, $"{group_id}", $"{phone_number.Replace("+", "")}", "Telegram.exe");
                    try
                    {
                        Process.Start(tg_path);
                    }
                    catch (Exception ex)
                    {
                        logger.err("Telegram port:", ex.Message);
                    }
                }
            });

            clearSessionCmd = ReactiveCommand.Create(() => {
                drop?.ClearSession();
            });

            removeCmd = ReactiveCommand.Create(() => {
                Status = DropStatus.removed;
                EventAggregator.getInstance().Publish((BaseEventMessage)new DropStatusChangedEventMessage(group_id, id, phone_number, DropStatus.removed, new() { }));
            });

            EventAggregator.getInstance().Subscribe(this);
        }

        private void Drop__2FAPasswordChanged(string new_password)
        {
            _2fa_password = new_password;

            using (var db = new DataBaseContext())
            {
                var found = db.Drops.FirstOrDefault(d => d.phone_number.Equals(phone_number));
                if (found != null)
                {
                    found._2fa_password = new_password;
                    db.SaveChanges();
                }
            }
        }

        private void Drop_ChannelMessageViewedEvent(long channel_id, uint counter)
        {
            EventAggregator.getInstance().Publish((BaseEventMessage)new ChannelMessageViewedEventMessage(channel_id, counter));
        }

        private void Drop_ChannelAddedEvent(string link, long channel_tg_id, string name)
        {

            using (var db = new DataBaseContext())
            {
                var channel = db.Channels.FirstOrDefault(c => c.link.Contains(link));
                if (channel != null) {
                    var found = db.DropSubscribes.FirstOrDefault(ds => ds.drop_id == id && ds.channel_id == channel.id);
                    if (found == null)
                    {
                        var ds = new DropSubscribeModel(id, channel.id);
                        db.DropSubscribes.Add(ds);
                        db.SaveChanges();

                        EventAggregator.getInstance().Publish((BaseEventMessage)new ChannelListUpdateRequestEventMessage(link, channel_tg_id, name));
                    }
                }                       
            }                        
        }

        private void Drop_ChannelLeftEvent(long channel_tg_id)
        {
            using (var db = new DataBaseContext())
            {
                var channel = db.Channels.FirstOrDefault(c => c.tg_id == channel_tg_id);
                if (channel != null)
                {
                    var found = db.DropSubscribes.FirstOrDefault(ds => ds.drop_id == id && ds.channel_id == channel.id);
                    if (found != null)
                    {
                        db.DropSubscribes.Remove(found);
                        db.SaveChanges();
                    }
                }

            }
        }

        ~dropVM()
        {
            drop?.Stop();
        }

        #region private
        private void Drop_StatusChangedEvent(ITGUser drop, DropStatus status)
        {            
            switch (status)
            {
                case DropStatus.active:
                    NeedVerification = false;
                    AllowStart = false;
                    break;
                case DropStatus.stopped:
                case DropStatus.revoked:
                    AllowStart = true;
                    break;
                case DropStatus.banned:
                    break;
                default:
                    break;
            }

            if (Status != status)
            {
                Status = status;                
                EventAggregator.getInstance().Publish((BaseEventMessage)new DropStatusChangedEventMessage(group_id, id, phone_number, Status, drop.GetSubscribes()));
            }
        }

        private void Drop_VerificationCodeRequestEvent(ITGUser obj)
        {
            NeedVerification = true;
        }
        #endregion

        #region public
        public async Task<bool> subscribe(string link)
        {
            if (Status == DropStatus.active)
            {
                using (var db = new DataBaseContext())
                {
                    var channel = db.Channels.FirstOrDefault(c => c.link.Contains(link));
                    if (channel != null)
                    {
                        var found = db.DropSubscribes.FirstOrDefault(ds => ds.drop_id == id && ds.channel_id == channel.id);
                        if (found == null)
                        {
                            await drop.Subscribe(link);
                            return true;
                        }
                        else
                        {
                            logger.inf(phone_number, "уже подписан");                         
                        }
                    }
                }               
            }
            return false;
        }

        public async void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case ChannelUnsubscibeEventMessage unsubscribeMesage:
                    await drop.Unsubscribe(unsubscribeMesage.tg_id);
                    break;

                case Change2FAPasswordAllEventMessage change2FAPasswordMessage:
                    if (drop._2fa_password.Equals(change2FAPasswordMessage.old_password))
                        drop.Change2FAPassword(change2FAPasswordMessage.old_password, change2FAPasswordMessage.new_password);
                    break;

                case Change2FAPasswordOneEventMessage change2FAPasswordOneEventMessage:
                    if (drop.phone_number.Equals(change2FAPasswordOneEventMessage.phone_number))
                    {
                        drop.Change2FAPassword(change2FAPasswordOneEventMessage.old_password, change2FAPasswordOneEventMessage.new_password);
                    }
                    break;

                case ChannelUnsubscribeRequestEventMessage unsubscribeMessage:
                    try
                    {
                        if (group_id == unsubscribeMessage.group_id)
                            await drop.Unsubscribe(unsubscribeMessage.tg_id);

                    }
                    catch (Exception ex)
                    {
                        logger.err(phone_number, $"OnEvent unsubscribeMessage: {ex.Message}");
                    }
                    break;              
            }
        }
        #endregion

        #region events
        public event Action<string, long, string> ChannelAddedEvent;
        #endregion
    }
}
