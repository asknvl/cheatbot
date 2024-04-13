using asknvl;
using asknvl.logger;
using cheatbot.Database;
using cheatbot.Models.polls;
using cheatbot.Models.reactions;
using cheatbot.Models.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TL;

namespace cheatbot.Models.drop
{
    public class Drop_v0 : TGUserBase
    {
        #region const
        int watch_percent = 20;
#if DEBUG_FAST
        int poll_percent = 30;
#else
        int poll_percent = 30;
#endif
        #endregion

        #region vars
        System.Timers.Timer readHistoryTimer;
        List<messageInfo> newMessagesQueue = new();
        ReactionsStateManager reactionsMansger = ReactionsStateManager.getInstance();

        System.Timers.Timer pollTimer;
        PollStateManager pollStateManager = PollStateManager.getInstance();
        List<pollInfo> newPollsQueue = new();

        Random random = new Random();
        #endregion

        public Drop_v0(string api_id, string api_hash, string phone_number, string old_2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, old_2fa_password, logger)
        {         
        }
#if DEBUG_FAST
        bool needFirstWatch = true;
#else
        bool needFirstWatch = true;
#endif
        async void ReadHistoryTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (needFirstWatch)
            {

                try
                {

                    await Task.Run(async () =>
                    {

                        var copy = chats.ToDictionary(e => e.Key, e => e.Value);

                        foreach (var channel in copy)
                        {
                            if (!acceptedIds.Contains(channel.Key))
                                continue;

                            if (channel.Value.IsChannel && channel.Value.IsActive)
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

                    if (chats.ContainsKey(m.peer_id))
                    {

                        InputPeer channel = chats[m.peer_id];

                        List<messageInfo> tmpList = new();
                        foreach (var item in newMessagesQueue)
                        {

                            if (item != null && item.peer_id == m.peer_id)
                                tmpList.Add(item);
                        }

                        foreach (var item in tmpList)
                            newMessagesQueue.Remove(item);

                        var messagesIDs = tmpList.Select(m => m.message_id).ToArray();

                        var history = await user.Messages_GetHistory(channel, limit: tmpList.Count);
                        var ids = history.Messages.Select(m => m.ID).ToArray();
                        await user.Messages_GetMessagesViews(channel, ids, true);
                        SendChannelMessageViewedEvent(m.peer_id, (uint)ids.Length);

                        int percentage = random.Next(1, 100);

                        if (percentage <= watch_percent)
                        {

                            var fullchat = await user.GetFullChat(channel);
                            var reactions = fullchat?.full_chat.AvailableReactions;

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

                    var id = unm.message.Peer.ID;
                    if (acceptedIds.Contains(id))
                    {
                        logger.inf($"{phone_number} update:", update.ToString());

                        var needWatch = await handleMessage(unm.message);

                        //if (needWatch)
                        encueueMessageToWatch(unm);
                    }
                    break;
            }
        }

        void encueueMessageToWatch(UpdateNewMessage newMessage)
        {
            var msgInfo = new messageInfo(newMessage);
            newMessagesQueue.Add(msgInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poll"></param>
        /// <param name="peer"></param>
        /// <param name="id"></param>
        /// <returns>true-нужно делать просмотр на это сообщение</returns>
        bool encueuePollMessage(MessageMediaPoll poll, Peer peer, int id)
        {
            bool res = true;
            int percentage = random.Next(1, 100);

#if DEBUG_FAST
            //test_mode = true;
            percentage = 0;
#endif

            if (percentage < poll_percent)
            {
                var pollInfo = new pollInfo(poll, peer, id);
                newPollsQueue.Add(pollInfo);
                res = false;
            }

            return res;
        }

        async Task handlePollMessage(MessageMediaPoll poll, Peer peer, int id)
        {
            await Task.Run(async () =>
            {

                InputPeer c = chats[peer.ID];
                await user.Messages_GetMessagesViews(c, new[] { id }, true);

                var answers = poll.poll.answers;
                pollStateManager.UpdatePollList(peer.ID, id, answers);

                //var inputPeer = dialogs.UserOrChat(peer).ToInputPeer();

                var inputPeer = chats[peer.ID].ToInputPeer();

                var state = pollStateManager.Get(peer.ID, id);
                var res = state.getAnswer();

                if (res != null)
                    await user.Messages_SendVote(inputPeer, id, res.option);

            });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageBase"></param>
        /// <returns>need watch</returns>
        async Task<bool> handleMessage(MessageBase messageBase)
        {
            var peer = messageBase.Peer;
            var id = messageBase.ID;
            bool res = true;

            switch (messageBase)
            {
                case Message message:
                    try
                    {
                        switch (message.media)
                        {

                            case MessageMediaPoll poll:
                                res = encueuePollMessage(poll, peer, id); //
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.err($"{phone_number}", $"{phone_number} handleMessage: {ex.Message}");
                    }
                    break;
            }

            await Task.CompletedTask;
            return res;
        }

        public override Task Start(string proxy)
        {
            return base.Start(proxy).ContinueWith(t =>
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

                    minOffset = random.Next(3, 10) + (1.0d * random.Next(1, 10) / 10);
#if DEBUG_FAST
                    offset = (int)(minOffset * 10 * 1000);
#else
                    offset = (int)(minOffset * 60 * 1000);
#endif

                    pollTimer = new System.Timers.Timer(offset);
                    pollTimer.AutoReset = true;
                    pollTimer.Elapsed += PollTimer_Elapsed;
                    pollTimer.Start();

                }
            });
        }

        private async void PollTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (newPollsQueue.Count > 0)
                {

                    int index = random.Next(0, newPollsQueue.Count);
                    var newPoll = newPollsQueue[index];
                    //var newPoll = newPollsQueue.First();                    
                    newPollsQueue.Remove(newPoll);
                    await handlePollMessage(newPoll.poll, newPoll.peer, newPoll.id);                 
                }
            }
            catch (Exception ex)
            {
                logger.err($"{phone_number}", $"PollTimer_Elpased: {ex.Message}");
            }
        }

        public override async Task Stop()
        {
            await base.Stop().ContinueWith(t =>
            {
                if (readHistoryTimer != null)
                {
                    readHistoryTimer.Stop();
                    readHistoryTimer.Elapsed -= ReadHistoryTimer_Elapsed;
                }

                if (pollTimer != null)
                {
                    pollTimer.Stop();
                    pollTimer.Elapsed -= PollTimer_Elapsed;
                }
            });

        }
    }   
}
