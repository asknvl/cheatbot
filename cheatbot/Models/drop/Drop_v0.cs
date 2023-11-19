using asknvl;
using asknvl.logger;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace cheatbot.Models.drop
{
    public class Drop_v0 : TGUserBase
    {
        #region vars
        System.Timers.Timer readHistoryTimer;
        #endregion

        public Drop_v0(string api_id, string api_hash, string phone_number, string old_2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, old_2fa_password, logger)
        {
            readHistoryTimer = new System.Timers.Timer();
            readHistoryTimer.Interval = 10000;
            readHistoryTimer.AutoReset = true;
            readHistoryTimer.Elapsed += ReadHistoryTimer_Elapsed;            
        }

        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var chats = await user.Messages_GetAllChats();
                
                if (newMessagesQueue.Count > 0)
                {
                    var m = newMessagesQueue[0];
                    InputPeer channel = chats.chats[m.Item1];
                    //var h = await user.Messages_ReadHistory(channel);
                    var v = await user.Messages_GetMessagesViews(channel, new int[] { m.Item2 }, true);
                    newMessagesQueue.RemoveAt(0);
                    SendChannelMessageViewedEvent(m.Item1);
                    logger.err(phone_number, $"Viewed {m.Item2}");
                }

                //foreach (var chat in chats.chats) {                    
                //    var history = await user.Messages_ReadHistory(chat);
                //    Thread.Sleep(10000);
                //}

            } catch (Exception ex)
            {
                if (newMessagesQueue.Count > 0)
                    newMessagesQueue.RemoveAt(0);

                logger.err("API", ex.Message);
            }
        }

        //Queue<(long, int)> newMessagesQueue = new Queue<(long, int)>();

        List<(long, int)> newMessagesQueue = new List<(long, int)>();

        protected override void processUpdate(Update update)
        {
            logger.inf(phone_number, update.ToString());
            switch (update)
            {
                case UpdateNewMessage unm:
                    newMessagesQueue.Add((unm.message.Peer.ID, unm.message.ID));
                    break;
            }
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t => {
                readHistoryTimer.Start();
            });
        }

        public override void Stop()
        {
            base.Stop();
            readHistoryTimer.Stop();
        }
    }
}
