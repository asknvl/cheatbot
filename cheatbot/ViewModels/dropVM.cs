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
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class dropVM : ViewModelBase, IEventSubscriber<ChannelUnsubscibeEvent>
    {
        #region vars
        IDropFactory dropFactory;
        ITGUser drop;        
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

        public dropVM(string phone_number, ILogger logger)
        {
            this.phone_number = phone_number;

            dropFactory = new DropFactory(logger);

            drop = dropFactory.Get(DropType.v0, phone_number);
            drop.ChannelAddedEvent += Drop_ChannelAddedEvent;


            drop.VerificationCodeRequestEvent += Drop_VerificationCodeRequestEvent;
            drop.StartedEvent += Drop_StartedEvent;
            drop.StoppedEvent += Drop_StoppedEvent;

            startCmd = ReactiveCommand.CreateFromTask(async () => {
                await drop.Start();
            });
            stopCmd = ReactiveCommand.Create(() => {
                drop.Stop();                
            });
            verifyCmd = ReactiveCommand.Create(() => {
                drop.SetVerifyCode(code);            
            });            


            EventAggregator.getInstance().Subscribe(this);
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

        public void OnEvent(ChannelUnsubscibeEvent message)
        {
            Task.Run(async () => {

                await drop.LeaveChannel(message.tg_id);
            
            });
        }
        #endregion

        #region events
        public event Action<string, long, string> ChannelAddedEvent;
        #endregion
    }
}
