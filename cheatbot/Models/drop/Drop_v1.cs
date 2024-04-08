using asknvl;
using asknvl.logger;
using cheatbot.Models.polls;
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
        int max_views_on_start = 4;
        #endregion

        #region vars
        System.Timers.Timer activityTimer;
        Random random = new Random();
        string proxy;
        bool isProcessing = false;
        List<channelState> channelsStates = new();
        PollStateManager pollStateManager = PollStateManager.getInstance();
        #endregion

        public Drop_v1(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, _2fa_password, logger)
        {
            activityTimer = new System.Timers.Timer();
            activityTimer.Interval = getPeriod();
            activityTimer.AutoReset = true;
            activityTimer.Elapsed += ActivityTimer_Elapsed;
            activityTimer.Start();
        }

        #region heplers
        int getPeriod()
        {
            return random.Next(1, 1) * 30 * 1000;
        }
        #endregion

        private async void ActivityTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (isProcessing)
                return;

            isProcessing = true;

            try
            {
                await user.Account_UpdateStatus(false);
                await handleUnread();
                await user.Account_UpdateStatus(true);
            }
            catch (Exception ex)
            {
                logger.err(phone_number, $"ActivityTimer {ex.Message}");
            }

            isProcessing = false;
        }


        int itterationNumber = 0;
        async Task handleUnread()
        {
            var accepted = await ChannelsProvider.getInstance().GetChannels();
            var dlgs = await user.Messages_GetAllDialogs();

            foreach (var item in dlgs.dialogs)
            {
                switch (dlgs.UserOrChat(item))
                {

                    case ChatBase chat when chat.IsActive:

                        if (/*accepted.Any(a => a.tg_id == chat.ID)*/true)
                        {


                            channelState? chState = channelsStates.FirstOrDefault(cs => cs.chat_id == chat.ID);
                            if (chState == null)
                            {
                                chState = new channelState()
                                {
                                    chat_id = chat.ID
                                };
                                channelsStates.Add(chState);
                            }


                            var full = await user.GetFullChat(chat);
                            var chf = (ChannelFull)full.full_chat;

                            chState.unread_prev = chState.unread;
                            chState.unread = chf.unread_count;


                            foreach (var cs in channelsStates)
                            {
                                logger.inf("TST", $"{cs.chat_id} unread:{cs.unread} unread_prev:{cs.unread_prev}");

                            }

                            //if (delta > 0)
                            //{
                                

                            //    var history = await user.Messages_GetHistory(chat, limit: delta);

                            //    var ids = history.Messages.Select(m => m.ID).ToArray();
                            //    await user.Messages_GetMessagesViews(chat, ids, true);

                            //}
                            //else
                            //    logger.inf("TST", $"{chat.Title} unread:{chf.unread_count} delta:{delta}");

                            //chats.Add(chat);
                        }
                        break;

                }
            }

            //var chts = dlgs.dialogs.Where(d => dlgs.UserOrChat(d) is ChatBase).ToList();
            //var chbs = chts[0] as ChatBase;

        }

        async Task handlePollMessage(MessageMediaPoll poll, Peer peer, int id)
        {
            await Task.Run(async () =>
            {

                var answers = poll.poll.answers;
                pollStateManager.UpdatePollList(peer.ID, id, answers);

                var inputPeer = chats[peer.ID].ToInputPeer();

                var state = pollStateManager.Get(peer.ID, id);
                var res = state.getAnswer();

                if (res != null)
                    await user.Messages_SendVote(inputPeer, id, res.option);
            });
        }

        protected override Task processUpdate(Update update)
        {
            return Task.CompletedTask;
        }
    }

    class channelState
    {
        public long chat_id { get; set; }
        public int unread { get; set;        }
        public int unread_prev { get; set; }
    }


}
