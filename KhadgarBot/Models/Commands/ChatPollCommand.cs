using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Threading;
using TwitchLib.Models.Client;

namespace KhadgarBot.Models
{
    public class ChatPollCommand : IChatCommand
    {
        #region Members

        private const int _chatPollTimerValue = 30000;

        private Timer _chatPollTimer = new Timer(_chatPollTimerValue);
        private Dictionary<string, int> _chatPollEntries = new Dictionary<string, int>();
        private bool _chatPollTimerIsRunning = false;
        private KhadgarBotViewModel _khadgarBotViewModel;

        #endregion

        #region Constructor

        public ChatPollCommand(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
            _chatPollTimer.Elapsed += onChatPollTimerElapsed;
        }

        #endregion

        #region Commands

        public bool CanProcess(ChatMessage chatMessage)
        {
            //if the command is running, there's no need to check if the message is the calling text
            //but still need to check the message for votes, so run it through the format checker, then return false
            //to avoid the other check
            if (_chatPollTimerIsRunning)
            {
                CheckMessageForChatPollFormatting(chatMessage.Channel, chatMessage.Username, chatMessage.Message);
                return false;
            }

            //specifically allow me to run commands regardless of my permissions.
            //this is only for development and testing, it will be removed once the bot gets to a good place
            if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni")
                    && chatMessage.Message[0] == '!' && chatMessage.Message == "!chatpoll" && !_chatPollTimerIsRunning)
            {
                _khadgarBotViewModel.SendChatMessage("The streamer has asked for a poll. Entries will be accepted for the next " + (_chatPollTimerValue / 1000).ToString() + " seconds.");

                _chatPollTimer.Start();
                _chatPollTimerIsRunning = true;
                _chatPollEntries.Clear();
                return true;
            }

            return false;
        }

        #endregion

        #region Methods



        private void onChatPollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _chatPollTimerIsRunning = false;
            _chatPollTimer.Stop();

            if (_chatPollEntries.Count == 0)
            {
                _khadgarBotViewModel.SendChatMessage("There were no entries in the poll.");
                return;
            }

            Dictionary<int, int> groupedEntries = _chatPollEntries.GroupBy(c => c.Value).ToDictionary(t => t.Key, t => t.Select(c => c.Key).Count());

            var maxVotes = groupedEntries.Aggregate((l, r) => l.Value > r.Value ? l : r).Value;
            var winners = groupedEntries.Where(g => g.Value == maxVotes);
            if (winners.Count() > 1)
            {
                var result = "";
                foreach (var winningEntry in winners)
                {
                    result += winningEntry.Key + ", ";
                }
                result = result.Substring(0, result.Length - 2);
                _khadgarBotViewModel.SendChatMessage(String.Format("There was a tie! The winners of the poll are {0} with {1} votes!", result, maxVotes));
            }
            else
            {
                _khadgarBotViewModel.SendChatMessage(String.Format("The winner of the poll is {0} with {1} votes!", winners.First().Key, maxVotes));
            }
        }

        private void CheckMessageForChatPollFormatting(string channel, string username, string message)
        {
            if (message.Length > 1)
                return;

            if (Int32.TryParse(message, out int vote))
            {
                if (!_chatPollEntries.ContainsKey(username))
                {
                    _chatPollEntries.Add(username, vote);
                }
                else
                {
                    _chatPollEntries[username] = vote;
                }
            }
        }

        #endregion
    }
}
