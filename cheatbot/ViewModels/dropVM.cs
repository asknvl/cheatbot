﻿using asknvl;
using asknvl.logger;
using Avalonia.Controls;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.Models.drop;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class dropVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        IDropFactory dropFactory;
        ITGUser drop;
        ILogger logger;
        #endregion

        #region properties
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

        bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set => this.RaiseAndSetIfChanged(ref isRunning, value);
        }

        bool needVerification;
        public bool NeedVerification
        {
            get => needVerification;
            set => this.RaiseAndSetIfChanged(ref needVerification, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> verifyCmd { get; }
        #endregion

        public dropVM(string phone_number, string _2fa_password, ILogger logger)
        {

            this.logger = logger;

            this.phone_number = phone_number;
            this._2fa_password = _2fa_password;

            dropFactory = new DropFactory(logger);

            drop = dropFactory.Get(DropType.v0, phone_number, _2fa_password);
            drop.ChannelAddedEvent += Drop_ChannelAddedEvent;
            drop.ChannelMessageViewedEvent += Drop_ChannelMessageViewedEvent;
            drop._2FAPasswordChanged += Drop__2FAPasswordChanged;


            drop.VerificationCodeRequestEvent += Drop_VerificationCodeRequestEvent;
            drop.StartedEvent += Drop_StartedEvent;
            drop.StoppedEvent += Drop_StoppedEvent;

            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await drop.Start();
            });
            stopCmd = ReactiveCommand.Create(() =>
            {
                drop.Stop();
            });
            verifyCmd = ReactiveCommand.Create(() =>
            {
                drop.SetVerifyCode(code);
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

        private void Drop_ChannelMessageViewedEvent(long channel_id)
        {
            EventAggregator.getInstance().Publish((BaseEventMessage)new ChannelMessageViewedEventMessage(channel_id));
        }

        private void Drop_ChannelAddedEvent(string link, long id, string name)
        {
            ChannelAddedEvent?.Invoke(link, id, name);
        }

        ~dropVM()
        {
            drop?.Stop();
        }

        private void Drop_StoppedEvent(ITGUser drop)
        {
            IsRunning = false;
        }

        #region private
        private void Drop_StartedEvent(ITGUser drop, bool result)
        {
            IsRunning = result;
            NeedVerification = false;
        }

        private void Drop_VerificationCodeRequestEvent(ITGUser obj)
        {
            NeedVerification = true;
        }
        #endregion

        #region public
        public async Task subscribe(string link)
        {
            if (IsRunning)
                await drop.Subscribe(link);
        }

        //public void OnEvent(ChannelUnsubscibeEvent message)
        //{
        //    Task.Run(async () => {

        //        await drop.LeaveChannel(message.tg_id);

        //    });
        //}

        public void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case ChannelUnsubscibeEventMessage unsubscribeMesage:
                    drop.LeaveChannel(unsubscribeMesage.tg_id);
                    break;

                case Change2FAPasswordAllEventMessage change2FAPasswordMessage:
                    if (drop._2fa_password.Equals(change2FAPasswordMessage.old_password))
                        drop.Change2FAPassword(change2FAPasswordMessage.old_password, change2FAPasswordMessage.new_password);
                    break;

                case Change2FAPasswordOneEventMessage change2FAPasswordOneEventMessage:
                    if (drop.phone_number.Equals(change2FAPasswordOneEventMessage.phone_number))
                    {
                        drop.Change2FAPassword(change2FAPasswordOneEventMessage.old_password, change2FAPasswordOneEventMessage.new_password);
                        //if (drop._2fa_password.Equals(change2FAPasswordOneEventMessage.old_password))
                        //    drop.Change2FAPassword(change2FAPasswordOneEventMessage.old_password, change2FAPasswordOneEventMessage.new_password);
                        //else
                        //    logger.err(phone_number, $"Old 2FA doesn't match current ({drop._2fa_password})");
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
