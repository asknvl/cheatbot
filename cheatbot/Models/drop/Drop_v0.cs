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
        List<messageInfo> newMessagesQueue = new();
        #endregion

        public Drop_v0(string api_id, string api_hash, string phone_number, string old_2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, old_2fa_password, logger)
        {            
        }


        Random r = new Random();
        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                var channel = chats.chats.ElementAt(r.Next(0, chats.chats.Count)).Value;
                var history = await user.Messages_GetHistory(channel, limit: 10);
                var ids = history.Messages.Select(m => m.ID).ToArray();
                await user.Messages_GetMessagesViews(channel, ids, true);

                //if (newMessagesQueue.Count > 0)
                //{

                //    int index = r.Next(0, newMessagesQueue.Count - 1);

                //    var m = newMessagesQueue[index];
                //    InputPeer channel = chats.chats[m.peer_id];

                //    List<messageInfo> tmpList = new();
                //    foreach (var item in newMessagesQueue)
                //    {
                //        if (item.peer_id == m.peer_id)
                //            tmpList.Add(item);
                //    }

                //    foreach (var item in tmpList)
                //        newMessagesQueue.Remove(item);

                //    var messages = tmpList.Select(m => m.message_id).ToArray();

                //    //var v = await user.Messages_GetMessagesViews(channel, messages, true);

                //    var history = await user.Messages_GetHistory(channel, limit: 50);

                //    var ids = history.Messages.Select(m => m.ID).ToArray();

                //    await user.Messages_GetMessagesViews(channel, ids, true);


                //    //var v = await user.Messages_GetMessagesViews(channel, new int[] { m.message_id }, true);
                //    //newMessagesQueue.RemoveAt(0);
                //    SendChannelMessageViewedEvent(m.peer_id, (uint)messages.Length);
                //    //logger.err(phone_number, $"Viewed {m.message_id}");
                //}

            } catch (Exception ex)
            {
                //if (newMessagesQueue.Count > 0)
                //    newMessagesQueue.RemoveAt(0);

                logger.err("WATHCES:", ex.Message);
            }
        }

        protected override async Task processUpdate(Update update)
        {
            logger.inf(phone_number, update.ToString());
            switch (update)
            {
                case UpdateNewMessage unm:

                    //var nm = (Message)unm.message;
                    //var found = false;

                    //if (nm.grouped_id != 0)
                    //    found = newMessagesQueue.Any(m => m.grouped_id == nm.grouped_id);

                    //if (!found)
                    //{
                    //    var msgInfo = new messageInfo(unm);
                    //    newMessagesQueue.Add(msgInfo);
                    //}


                    //var msgInfo = new messageInfo(unm);
                    //newMessagesQueue.Add(msgInfo);

                    break;

                case UpdateChannel uch:
                    try
                    {
                        chats = await user.Messages_GetAllChats();
                    } catch (Exception ex)
                    {
                        logger.err(phone_number, $"processUpdate: {ex.Message}");
                    }
                    break;
            }
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t => {

                if (is_active)
                {

                    Random r = new Random();

                    //int offset = (r.Next(1, 9) * 1000 + r.Next(1, 10) * 100) * 60;
                    //double minOffset = (double)offset / 1000 / 60;


                    double minOffset = r.Next(1, 10) + (1.0d * r.Next(1, 10) / 10);
                    int offset = (int)(minOffset * 60 * 1000);

                    logger.inf("", $"minoffset={minOffset} offset={offset}");

                    offset = 5000;
                    readHistoryTimer = new System.Timers.Timer(offset);
                    //readHistoryTimer = new System.Timers.Timer(minuteOffset * 60 * 1000);
                    //logger.inf("", "minuteOffset=" + minuteOffset);
                    readHistoryTimer.AutoReset = true;
                    readHistoryTimer.Elapsed += ReadHistoryTimer_Elapsed;
                    readHistoryTimer.Start();
                }
            });
        }

        public override async Task Stop()
        {
            await base.Stop().ContinueWith(t => {
                if (readHistoryTimer != null)
                {
                    readHistoryTimer?.Stop();
                    readHistoryTimer.Elapsed -= ReadHistoryTimer_Elapsed;
                }
            });
            
        }
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
    }
}
