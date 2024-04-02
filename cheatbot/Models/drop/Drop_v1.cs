using asknvl;
using asknvl.logger;
using cheatbot.Models.server;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TL;

namespace cheatbot.Models.drop
{
    public class Drop_v1 : TGUserBase
    {

        #region const
        int last_vieved_number;
        #endregion

        #region vars
        System.Timers.Timer activityTimer;
        Random random = new Random();
        string proxy;
        bool isProcessing = false;
        List<itteration> itterations = new();
        #endregion

        public Drop_v1(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, _2fa_password, logger)
        {
            activityTimer = new System.Timers.Timer();
            activityTimer.Interval = getPeriod();
            activityTimer.AutoReset = true;
            activityTimer.Elapsed += ActivityTimer_Elapsed;            
        }

        #region heplers
        int getPeriod()
        {
            return random.Next(1, 1) * 60 *  1000;
        }
        #endregion

        private async void ActivityTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (isProcessing)
                return;

            isProcessing = true;    

            try
            {
                await base.Start(proxy);
                //await Task.Delay(5000);

                await watchUnread();

                await base.Stop();

            } catch (Exception ex)
            {
                logger.err(phone_number, $"ActivityTimer {ex.Message}");
            }

            isProcessing = false;
        }

        async Task watchUnread()
        {
            var accepted = await ChannelsProvider.getInstance().GetChannels();
            var dlgs = await user.Messages_GetAllDialogs();

            List<ChatBase> chats = new List<ChatBase>();

            foreach (var item in dlgs.dialogs)
            {
                switch (dlgs.UserOrChat(item)) {

                    case ChatBase chat:

                        if (/*accepted.Any(a => a.tg_id == chat.ID)*/true)
                        {

                            itteration? found = itterations.FirstOrDefault(i => i.chat_id == chat.ID);
                            if (found == null)
                            {
                                found = new itteration()
                                {
                                    chat_id = chat.ID,
                                    number = 0
                                };
                                itterations.Add(found);                                
                            }

                            var full = await user.GetFullChat(chat);
                            var chf = (ChannelFull)full.full_chat;
                            if (chf.unread_count > 0)
                            {
                                logger.inf("TST", $"{chat.Title} {chf.unread_count}");
                                chats.Add(chat);

                                var history = await user.Messages_GetHistory(chat, limit: chf.unread_count);
                                var ids = history.Messages.Select(m => m.ID).ToArray();

                                await user.Messages_GetMessagesViews(chat, ids, true);

                            }
                        }
                        break;

                }                
            }

            //var chts = dlgs.dialogs.Where(d => dlgs.UserOrChat(d) is ChatBase).ToList();
            //var chbs = chts[0] as ChatBase;
            
        }

        protected override Task processUpdate(Update update)
        {
            return Task.CompletedTask;
        }

        public override Task Start(string proxy)
        {
            this.proxy = proxy;            
            activityTimer.Start();
            return Task.CompletedTask;
        }
    }

    class itteration
    {
        public long chat_id { get; set; }
        public int number { get; set; }
        public int unread_prev { get; set; }
    }


}
