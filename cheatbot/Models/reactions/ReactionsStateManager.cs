using cheatbot.Models.drop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace cheatbot.Models.reactions
{
    public class ReactionsStateManager
    {
        #region const
        int max_messages_states = 2048;
        #endregion

        #region vars
        List<messageState> messageStates = new(); 
        #endregion

        #region singletone
        private static ReactionsStateManager instance;
        private ReactionsStateManager() { 
        }

        public static ReactionsStateManager getInstance()
        {
            if (instance == null)
                instance = new ReactionsStateManager();
            return instance;
        }
        #endregion

        #region public        
        public void UpdateMessageList(long channelID, int messageID, Reaction[] available)
        {
            try
            {
                var found = messageStates.Any(ms => ms.channelID == channelID && ms.messageID == messageID);
                if (!found)
                {
                    try
                    {
                        var messageState = new messageState(channelID, messageID, available);
                        messageStates.Add(messageState);
                        if (messageStates.Count > max_messages_states)
                            messageStates.RemoveAt(0);
                    }
                    catch (Exception ex)
                    {
                        //throw new Exception($"UpdateMessageList {ex.Message}");
                    }
                }
            } catch (Exception ex)
            {

            }
        }


        public messageState? Get(long channelID, int messageID) {
            var found = messageStates.FirstOrDefault(ms => ms.channelID == channelID && ms.messageID == messageID);
            return found;
        }

        #endregion
    }

    public class messageState
    {
        public long channelID { get; set; }
        public int messageID { get; set; }
        public Reaction[] reactions { get; set; } = new Reaction[0];

        public messageState(long channelID, int messageID, Reaction[] available_reactions)
        {
            this.channelID = channelID;
            this.messageID = messageID;
            reactions = getReactionsSeq(available_reactions);
        }   

        Reaction[] getReactionsSeq(Reaction[] available)
        {
            
            Reaction[] res = new Reaction[available.Length];

            Random r = new Random();
            int[] randSeq = Enumerable.Range(0, available.Length).OrderBy(x => r.Next()).ToArray();

            for (int index = 0; index < randSeq.Length; index++)
            {
                res[index] = available[randSeq[index]];
            }
            return res;
        }
    }
         
}
