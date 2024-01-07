using asknvl;
using asknvl.logger;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
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

        bool needFirstWatch = false;
        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (needFirstWatch) {

                needFirstWatch = false;

                await Task.Run(async () => { 

                    foreach (var channel in chats.chats)
                    {

                        if (channel.Value.IsChannel)
                        {

                            try
                            {
                                var history = await user.Messages_GetHistory(channel.Value, limit: 8);
                                var ids = history.Messages.Select(m => m.ID).ToArray();
                                await user.Messages_GetMessagesViews(channel.Value, ids, true);

                                Thread.Sleep(10000);
                            }
                            catch (Exception ex)
                            {
                                logger.err("ReadHistoryTimer FirstView:", ex.Message);
                            }
                        }

                    }

                });


                
            }


            try
            {
                if (newMessagesQueue.Count > 0)
                {

                    int index = r.Next(0, newMessagesQueue.Count - 1);

                    var m = newMessagesQueue[index];
                    InputPeer channel = chats.chats[m.peer_id];

                    List<messageInfo> tmpList = new();
                    foreach (var item in newMessagesQueue)
                    {
                        if (item.peer_id == m.peer_id)
                            tmpList.Add(item);
                    }

                    foreach (var item in tmpList)
                        newMessagesQueue.Remove(item);

                    var messages = tmpList.Select(m => m.message_id).ToArray();

                    var v = await user.Messages_GetMessagesViews(channel, messages, true);

                    //var v = await user.Messages_GetMessagesViews(channel, new int[] { m.message_id }, true);
                    //newMessagesQueue.RemoveAt(0);
                    SendChannelMessageViewedEvent(m.peer_id, (uint)messages.Length);
                    //logger.err(phone_number, $"Viewed {m.message_id}");

                    int percentage = r.Next(1, 100);

                    if (test_mode && percentage <= 20)                    {

                        //var reactions = fullChats.FirstOrDefault(c => c.chats.ContainsKey(channel.ID))?.full_chat.AvailableReactions;
                        var fullchat = await user.GetFullChat(channel);
                        var reactions = fullchat?.full_chat.AvailableReactions;

                        if (reactions is ChatReactionsSome)
                        {
                            var some = (ChatReactionsSome)reactions;
                            var available = some.reactions;

                            int selected = 0;
                            percentage = r.Next(1, 100);

                            if (percentage <= 40)
                                selected = 0;
                            else
                                if (available.Length > 2 && percentage > 40 && percentage <= 70)
                                selected = 1;
                            else
                                if (available.Length > 3)
                                selected = r.Next(2, available.Length-1);

                            foreach (var message in messages)
                            {
                                await user.Messages_SendReaction(channel, message, reaction: new[] { available[selected] });
                                await Task.Delay(5000);
                            }

                        }
                    }
                }

            } catch (Exception ex)
            {
                if (newMessagesQueue.Count > 0)
                    newMessagesQueue.RemoveAt(0);

                logger.err("ReadHistoryTimer View:", ex.Message);
            }
        }

        protected override async Task processUpdate(Update update)
        {
            //logger.inf(phone_number, update.ToString());
            switch (update)
            {
                case UpdateNewMessage unm:

                    var nm = (Message)unm.message;
                    var found = false;

                    if (nm.grouped_id != 0)
                        found = newMessagesQueue.Any(m => m.grouped_id == nm.grouped_id);

                    if (!found)
                    {
                        var msgInfo = new messageInfo(unm);
                        newMessagesQueue.Add(msgInfo);
                    }
                    break;

                case UpdateChannel uch:
                    try
                    {
                        //chats = await user.Messages_GetAllChats();
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
