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
        System.Timers.Timer reactionsTimer;
        List<messageInfo> newMessagesViewsQueue = new();       


        List<messageInfo> newMessagesReactionsQueue = new();
        
        #endregion

        public Drop_v0(string api_id, string api_hash, string phone_number, string old_2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, old_2fa_password, logger)
        {            
        }


        Random r = new Random();
        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (newMessagesViewsQueue.Count > 0)
                {

                    int index = r.Next(0, newMessagesViewsQueue.Count - 1);

                    var m = newMessagesViewsQueue[index];
                    InputPeer channel = chats.chats[m.peer_id];

                    List<messageInfo> tmpList = new();
                    foreach (var item in newMessagesViewsQueue)
                    {
                        if (item.peer_id == m.peer_id)
                            tmpList.Add(item);
                    }

                    foreach (var item in tmpList)
                        newMessagesViewsQueue.Remove(item);

                    var messages = tmpList.Select(m => m.message_id).ToArray();
                    var v = await user.Messages_GetMessagesViews(channel, messages, true);

                    SendChannelMessageViewedEvent(m.peer_id, (uint)messages.Length);

                    var reactions = fullChats.FirstOrDefault(c => c.chats.ContainsKey(channel.ID))?.full_chat.AvailableReactions;

                    Reaction reaction = reactions switch
                    {
                        ChatReactionsSome some => some.reactions[0],
                        _ => null
                    };

                    if (reaction == null)
                        return;
                    
                    foreach (var message in messages)
                    {
                        await user.Messages_SendReaction(channel, message, reaction: new[] { reaction });
                    }
                }

            } catch (Exception ex)
            {
                if (newMessagesViewsQueue.Count > 0)
                    newMessagesViewsQueue.RemoveAt(0);

                logger.err("API", ex.Message);
            }
        }

        protected override async Task processUpdate(Update update)
        {
            logger.inf(phone_number, update.ToString());
            switch (update)
            {
                //case UpdateNewChannelMessage nchm:
                //    break;

                case UpdateNewMessage unm:

                    var nm = (Message)unm.message;
                    var found = false;

                    if (nm.grouped_id != 0)
                        found = newMessagesViewsQueue.Any(m => m.grouped_id == nm.grouped_id);

                    if (!found)
                    {
                        var msgInfo = new messageInfo(unm);
                        newMessagesViewsQueue.Add(msgInfo);
                        newMessagesReactionsQueue.Add(msgInfo);
                    }
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

                    double minOffset = r.Next(1, 10) + (1.0d * r.Next(1, 10) / 10);
                    int offset = (int)(minOffset * 60 * 1000);

                    offset = 1000;

                    logger.inf("", $"minoffset={minOffset} offset={offset}");

                    readHistoryTimer = new System.Timers.Timer(offset);
                    readHistoryTimer.AutoReset = true;
                    readHistoryTimer.Elapsed += ReadHistoryTimer_Elapsed;
                    readHistoryTimer.Start();

                    reactionsTimer = new System.Timers.Timer(1000);
                    reactionsTimer.AutoReset = true;
                    reactionsTimer.Elapsed += ReactionsTimer_Elapsed;
                    reactionsTimer.Start();

                }
            });
        }

        private void ReactionsTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {            

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
