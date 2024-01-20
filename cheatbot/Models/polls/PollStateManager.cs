using cheatbot.Models.reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace cheatbot.Models.polls
{
    public class PollStateManager
    {
        #region const
        int max_polls_states = 2048;
        #endregion

        #region vars
        List<pollState> pollsStates = new();
        #endregion

        #region singletone
        private static PollStateManager instance;
        private PollStateManager()
        {
        }

        public static PollStateManager getInstance()
        {
            if (instance == null)
                instance = new PollStateManager();
            return instance;
        }
        #endregion

        #region public       
        public void UpdatePollList(long channelID, int messageID, PollAnswer[] answers)
        {
            var found = pollsStates.Any(p => p.channelID == channelID && p.messageID == messageID);
            if (!found) {
                try
                {
                    var pollState = new pollState(channelID, messageID, answers);   
                    pollsStates.Add(pollState);
                    if (pollsStates.Count > max_polls_states)
                        pollsStates.RemoveAt(0);
                } catch (Exception ex)
                {
                    throw new Exception($"UpdatePollList {ex.Message}");
                }
            }
        }

        public pollState? Get(long channelID, long messageID)
        {
            var found = pollsStates.FirstOrDefault(p => p.channelID == channelID && p.messageID == messageID);
            return found;
        }
        #endregion
    }

    public class pollState
    {
        #region vars
        int topIndex;
        Random random = new Random();
        int top_1_percentage;
        int top_2_percentage;
        #endregion

        #region properties
        public long channelID { get; set; }
        public int messageID { get; set; }
        public PollAnswer[] answers { get; set; }
        #endregion

        public pollState(long channelID, int messageID, PollAnswer[] answers)
        {
            this.channelID = channelID;
            this.messageID = messageID;
            this.answers = answers;

            //topIndex = random.Next(0, answers.Length);

            this.answers = answers.OrderBy(a => random.Next()).ToArray();
            top_1_percentage = random.Next(5, 8) * 10;
            top_2_percentage = top_1_percentage - random.Next(0, 11); 
        }

        public PollAnswer getAnswer()
        {
            int percentage = random.Next(1, 100);
            if (percentage <= top_1_percentage)
            {
                return answers[0];
            }
            else
                if (percentage > top_1_percentage && percentage <= top_1_percentage + top_2_percentage)
                return answers[1];
            else                
                return answers[random.Next(2, answers.Length)];                
        }
    }
}
