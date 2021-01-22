using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using System;
using System.Text.RegularExpressions;

namespace KhadgarBot.Models.Commands
{
    public class PredsCommand : IChatCommand
    {
        #region Members

        private int _predsTimerValue = 30;
        private int _cooldownTimerValue = 1;//60000;//60 seconds

        private Timer _predsTimer = new Timer();
        private Timer _cooldownTimer = new Timer();
        private bool _predsTimerIsRunning = false;
        private bool _onCooldown = false;
        private bool _winnerWasDeclared = false;
        private Dictionary<string, double> _predsEntries = new Dictionary<string, double>();
        private KhadgarBotViewModel _khadgarBotViewModel;

        #endregion

        #region Constructor

        public PredsCommand(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
            _predsTimer.Elapsed += onPredsTimerElapsed;
            _cooldownTimer = new Timer(_cooldownTimerValue);
            _cooldownTimer.Elapsed += onCooldownTimerElapsed;
        }

        #endregion


        #region Commands

        public bool CanProcess(ChatMessage chatMessage)
        {
            return CanProcessAsync(chatMessage).Result;
        }

        //not totally happy with how i have this working now, i want to make it a bit cleaner to be able to accept multiple command strings with different permission levels
        public async Task<bool> CanProcessAsync(ChatMessage chatMessage)
        {
            if (!_onCooldown)
            {
                //have a killswitch for a poll in case it needs to be cut short
                if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni"))
                {
                    if (_predsTimerIsRunning && chatMessage.Message.Length >= 12 && chatMessage.Message.Substring(0, 12).ToLower() == "!cancelpreds")
                    {
                        _predsTimerIsRunning = false;
                        _onCooldown = false;
                        _winnerWasDeclared = false;
                        _predsTimer.Stop();
                        _khadgarBotViewModel.SendChatMessage("Preds were canceled.");
                        //_khadgarBotViewModel.StopPreds();
                        return true;
                    }
                }

                //specifically allow me to run commands regardless of my permissions.
                //this is only for development and testing, it will be removed once the bot gets to a good place
                if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni"))
                {
                    if (!_predsTimerIsRunning && chatMessage.Message.Length == 6 && chatMessage.Message.ToLower() == "!preds")
                    {
                        _predsTimer.Interval = _predsTimerValue * 1000;

                        _khadgarBotViewModel.SendChatMessage($"Enter your predictions now!  You have {_predsTimerValue.ToString()} seconds!");
                        _predsTimer.Start();
                        _predsTimerIsRunning = true;
                        _winnerWasDeclared = false;
                        _predsEntries.Clear();
                        //_khadgarBotViewModel.StartPreds();
                        return true;
                    }
                }

                
            }

            //if the command is running, there's no need to check if the message is the calling text
            if (_predsTimerIsRunning && Double.TryParse(chatMessage.Message, out double userPred))
            //(Regex.Match(chatMessage.Message, @"\d\d\.\d").Success || Regex.Match(chatMessage.Message, @"\d\d\.\d\d").Success))
            {
                PredEntry(chatMessage.Username, userPred);
                return true;
            }

            if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni"))
            {
                if (!_predsTimerIsRunning && chatMessage.Message.Length >= 12 && chatMessage.Message.Substring(0, 12).ToLower() == "!predswinner" && !_winnerWasDeclared)
                {
                    if (Double.TryParse(chatMessage.Message.Split(' ')[1], out double actualTime))
                    {
                        GetPredsWinner(actualTime);
                    }
                    //_khadgarBotViewModel.StartPreds();
                    return true;
                }
            }

            //TEST CODE
            //if ((chatMessage.IsModerator || chatMessage.IsBroadcaster || chatMessage.Username == "ciarenni"))
            //{
            //    if (!_predsTimerIsRunning && chatMessage.Message.Length == 6 && chatMessage.Message.ToLower() == "!preds")
            //    {
            //        _predsTimer.Interval = _predsTimerValue * 1000;

            //        _khadgarBotViewModel.SendChatMessage("On cooldown");
            //        //_khadgarBotViewModel.StartPreds();
            //        return true;
            //    }
            //}

            return false;
        }

        #endregion

        #region Methods

        private void GetPredsWinner(double actualTime)
        {
            double difference = double.MaxValue;
            var winners = new Dictionary<string, double>();

            foreach(var entry in _predsEntries)
            {
                if(Math.Abs(actualTime - entry.Value) < difference)
                {
                    difference = Math.Abs(actualTime - entry.Value);
                }
            }

            foreach (var entry in _predsEntries)
            {
                if (Math.Abs(actualTime - entry.Value) == difference)
                {
                    winners.Add(entry.Key, entry.Value);
                }
            }

            var winnerMessage = new StringBuilder();
            winnerMessage.Append("Congratulations to ");
            bool first = true;
            foreach(var winner in winners)
            {
                if(first)
                {
                    winnerMessage.Append($"{winner.Key} with a {winner.Value.ToString()}");
                    first = false;
                }
                else
                {
                    winnerMessage.Append($", {winner.Key} with a {winner.Value.ToString()}");
                }
            }
            winnerMessage.Append("!");
            _khadgarBotViewModel.SendChatMessage(winnerMessage.ToString());
            _winnerWasDeclared = true;
        }

        private void onCooldownTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _onCooldown = false;
        }

        private void onPredsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _predsTimerIsRunning = false;
            _predsTimer.Stop();

            _cooldownTimer.Start();
            _onCooldown = true;

            if (_predsEntries.Count == 0)
            {
                _khadgarBotViewModel.SendChatMessage("No preds were made.");
            }
            else
            {
                _khadgarBotViewModel.SendChatMessage("Prediction entries are now closed!  Good luck!");

                //TEST CODE
                //StringBuilder resultsBuilder = new StringBuilder();
                //foreach (var entry in _predsEntries)
                //{
                //    resultsBuilder.AppendLine($"User: {entry.Key}, Guess: {entry.Value}");
                //}

                //if (resultsBuilder.ToString().Length > 500)
                //{
                //    _khadgarBotViewModel.SendChatMessage(resultsBuilder.ToString().Substring(0, 500));
                //}
                //else
                //{
                //    _khadgarBotViewModel.SendChatMessage(resultsBuilder.ToString());
                //}
            }

            //_khadgarBotViewModel.StopPreds();

            //Dictionary<int, int> groupedEntries = _predsEntries.GroupBy(c => c.Value).ToDictionary(t => t.Key, t => t.Select(c => c.Key).Count());

            //var maxVotes = groupedEntries.Aggregate((l, r) => l.Value > r.Value ? l : r).Value;
            //var winners = groupedEntries.Where(g => g.Value == maxVotes);
            //if (winners.Count() > 1)
            //{
            //    var result = "";
            //    foreach (var winningEntry in winners)
            //    {
            //        result += winningEntry.Key + ", ";
            //    }
            //    result = result.Substring(0, result.Length - 2);
            //    _khadgarBotViewModel.SendChatMessage(String.Format("There was a tie! The winners of the poll are {0} with {1} votes!", result, maxVotes));
            //}
            //else
            //{
            //    _khadgarBotViewModel.SendChatMessage(String.Format("The winner of the poll is {0} with {1} votes!", winners.First().Key, maxVotes));
            //}
        }

        private void PredEntry(string username, double pred)
        {
            if (!_predsEntries.ContainsKey(username))
            {
                _predsEntries.Add(username, pred);
            }
            else
            {
                _predsEntries[username] = pred;
            }
        }

        #endregion
    }
}
