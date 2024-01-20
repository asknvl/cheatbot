using asknvl;
using asknvl.logger;
using cheatbot.Models.polls;
using cheatbot.Models.reactions;
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
        #region const
        int watch_percent = 20;
        int poll_percent = 20;
        #endregion

        #region vars
        System.Timers.Timer readHistoryTimer;
        List<messageInfo> newMessagesQueue = new();
        ReactionsStateManager reactionsMansger = ReactionsStateManager.getInstance();
        PollStateManager pollStateManager = PollStateManager.getInstance();
        Random random = new Random();
        #endregion

        public Drop_v0(string api_id, string api_hash, string phone_number, string old_2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, old_2fa_password, logger)
        {
        }

        bool needFirstWatch = true;
        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (needFirstWatch)
            {

                try
                {

                    await Task.Run(async () =>
                     {

                         foreach (var channel in chats.chats)
                         {

                             if (channel.Value.IsChannel)
                             {

                                 try
                                 {
                                     var history = await user.Messages_GetHistory(channel.Value, limit: 4);
                                     var ids = history.Messages.Select(m => m.ID).ToArray();

                                     foreach (var id in ids)
                                     {
                                         messageInfo mi = new messageInfo(channel.Value.ID, id);
                                         newMessagesQueue.Add(mi);
                                     }
                                     //await user.Messages_GetMessagesViews(channel.Value, ids, true);
                                     //Thread.Sleep(5000);
                                     await Task.Delay(7000);
                                 }
                                 catch (Exception ex)
                                 {
                                     logger.err("ReadHistoryTimer FirstView:", ex.Message);
                                 }
                             }
                         }
                     });
                }
                catch (RpcException ex)
                {
                    processRpcException(ex);
                }
                catch (Exception ex)
                {
                    logger.err("ReadHistoryTimer FirstView:", ex.Message);
                }
            }

            if (needFirstWatch)
            {
                needFirstWatch = false;
                return;
            }

            try
            {               

                if (newMessagesQueue.Count > 0)
                {

                    int index = random.Next(0, newMessagesQueue.Count - 1);

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

                    var messagesIDs = tmpList.Select(m => m.message_id).ToArray();
                    //var v = await user.Messages_GetMessagesViews(channel, messagesIDs, true);
                    //SendChannelMessageViewedEvent(m.peer_id, (uint)messagesIDs.Length);

                    var history = await user.Messages_GetHistory(channel, limit: tmpList.Count);
                    var ids = history.Messages.Select(m => m.ID).ToArray();
                    await user.Messages_GetMessagesViews(channel, ids, true);
                    SendChannelMessageViewedEvent(m.peer_id, (uint)ids.Length);

                    int percentage = random.Next(1, 100);

                    if (percentage <= watch_percent)
                    {

                        var fullchat = await user.GetFullChat(channel);
                        var reactions = fullchat?.full_chat.AvailableReactions;
                        //var reactions = fullChats.FirstOrDefault(c => c.chats.ContainsKey(channel.ID))?.full_chat.AvailableReactions;

                        if (reactions is ChatReactionsSome)
                        {
                            var some = (ChatReactionsSome)reactions;
                            var available = some.reactions;

                            foreach (var messageID in messagesIDs)
                            {
                                try
                                {
                                    reactionsMansger.UpdateMessageList(channel.ID, messageID, available);
                                }
                                catch (Exception ex)
                                {
                                    logger.err("ReadHistoryTimer View:", ex.Message);
                                }

                                var messageState = reactionsMansger.Get(channel.ID, messageID);

                                if (messageState != null)
                                {


                                    Reaction[]? randomizedReactions = messageState.reactions;

                                    //string s = "";
                                    //foreach (var reaction in randomizedReactions)
                                    //{
                                    //    s = s + $" {((ReactionEmoji)reaction)?.emoticon}";                                     
                                    //}
                                    //logger.inf("rndmized:", s);

                                    if (randomizedReactions.Length > 0)
                                    {
                                        int selected = 0;
                                        percentage = random.Next(1, 100);

                                        if (percentage <= 40)
                                            selected = 0;
                                        else
                                            if (available.Length > 2 && percentage > 40 && percentage <= 70)
                                            selected = 1;
                                        else
                                            if (available.Length > 3)
                                            selected = random.Next(2, available.Length);

                                        await user.Messages_SendReaction(channel, messageID, reaction: new[] { randomizedReactions[selected] });
                                        await Task.Delay(10000);

                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (RpcException ex)
            {
                processRpcException(ex);
            }
            catch (Exception ex)
            {
                //if (newMessagesQueue.Count > 0)
                //    newMessagesQueue.RemoveAt(0);

                logger.err("ReadHistoryTimer View:", ex.Message);
            }
        }

        protected override async Task processUpdate(Update update)
        {
            switch (update)
            {
                case UpdateNewMessage unm:

                    logger.inf("update:", update.ToString());

                    encueueMessageToWatch(unm);

                    await handleMessage(unm.message);

                    //var nm = (Message)unm.message;
                    //var found = false;

                    //if (nm.grouped_id != 0)
                    //    found = newMessagesQueue.Any(m => m.grouped_id == nm.grouped_id);

                    //if (!found)
                    //{
                    //    var msgInfo = new messageInfo(unm);
                    //    newMessagesQueue.Add(msgInfo);
                    //}

            

                    break;
            }
        }

        void encueueMessageToWatch(UpdateNewMessage newMessage)
        {
            var msgInfo = new messageInfo(newMessage);
            newMessagesQueue.Add(msgInfo);
        }

        async Task handlePollMessage(MessageMediaPoll poll, Peer peer, int id)
        {
            int percentage = random.Next(1, 100);

#if DEBUG
            //test_mode = true;
            //percentage = 0;
#endif

            if (test_mode && percentage < poll_percent)
            {

                await Task.Run(async () => {


                    var answers = poll.poll.answers;
                    pollStateManager.UpdatePollList(peer.ID, id, answers);

                    int nxt = random.Next(1, 10);

                    await Task.Delay(nxt * 10 * 1000); 

                    var inputPeer = dialogs.UserOrChat(peer).ToInputPeer();

                    var state = pollStateManager.Get(peer.ID, id);

                    var res = state.getAnswer();

                    if (res != null)
                        await user.Messages_SendVote(inputPeer, id, res.option);

                });
            }
        }

        async Task handleMessage(MessageBase messageBase)
        {
            var peer = messageBase.Peer;
            var id = messageBase.ID;

            switch (messageBase)
            {
                case Message message:
                    try
                    {
                        switch (message.media) {

                            case MessageMediaPoll poll:
                                handlePollMessage(poll, peer, id);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    break;
            }
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t =>
            {
                if (status == DropStatus.active)
                {

                    double minOffset = random.Next(2, 7) + (1.0d * random.Next(1, 10) / 10);
#if DEBUG_FAST
                    int offset = (int)(minOffset * 10 * 1000);
#else
                    int offset = (int)(minOffset * 60 * 1000);
#endif

                    readHistoryTimer = new System.Timers.Timer(offset);
                    readHistoryTimer.AutoReset = true;
                    readHistoryTimer.Elapsed += ReadHistoryTimer_Elapsed;
                    readHistoryTimer.Start();
                }
            });
        }

        public override async Task Stop()
        {
            await base.Stop().ContinueWith(t =>
            {
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

        public messageInfo(long peer_id, int message_id)
        {
            this.peer_id = peer_id;
            this.message_id = message_id;
        }
    }
}
