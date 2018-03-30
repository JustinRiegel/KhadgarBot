using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Threading;
using TwitchLib.Models.Client;

namespace KhadgarBot.Models.Commands
{
    public class ChatPollCommand : IChatCommand
    {
        #region Members

        private int _chatPollTimerValue = 30;

        private Timer _chatPollTimer = new Timer();
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

        //not totally happy with how i have this working now, i want to make it a bit cleaner to be able to accept multiple command strings with different permission levels
        public bool CanProcess(ChatMessage chatMessage)
        {
            //have a killswitch for a poll in case it needs to be cut short
            if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni"))
            {
                if (chatMessage.Message.Length >= 11 && chatMessage.Message.Substring(0, 11).ToLower() == "!cancelpoll" && _chatPollTimerIsRunning)
                {
                    _chatPollTimerIsRunning = false;
                    _chatPollTimer.Stop();
                    _khadgarBotViewModel.SendChatMessage("The poll was canceled.");
                    return true;
                }
            }

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
            if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.IsSubscriber || chatMessage.Username == "ciarenni"))
            {
                if (chatMessage.Message.Length >= 9 && chatMessage.Message.Substring(0, 9).ToLower() == "!chatpoll" && !_chatPollTimerIsRunning)
                {
                    _chatPollTimerValue = 30;

                    //check if the message has at least 2 parts split by a space, meaning a (hopefully) integer value was passed in to set the timer length
                    if (chatMessage.Message.Split(' ').Count() > 1)
                    {
                        //check if it was an integer. if it wasn't, the 30 that was set above should hold
                        Int32.TryParse(chatMessage.Message.Split(' ')[1], out _chatPollTimerValue);

                        //if the timer value was set too high or low, set it to 30 instead
                        if (_chatPollTimerValue <= 15 || _chatPollTimerValue > 180)
                        {
                            _chatPollTimerValue = 30;
                        }
                    }

                    _chatPollTimer.Interval = _chatPollTimerValue * 1000;

                    _khadgarBotViewModel.SendChatMessage("The streamer has asked for a poll. Entries will be accepted for the next " + _chatPollTimerValue.ToString() + " seconds.");
                    _chatPollTimer.Start();
                    _chatPollTimerIsRunning = true;
                    _chatPollEntries.Clear();
                    return true;
                }
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
