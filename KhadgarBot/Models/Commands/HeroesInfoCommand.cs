using KhadgarBot.Interfaces;
using KhadgarBot.ViewModels;
using System.Text.RegularExpressions;
//using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System;

namespace KhadgarBot.Models.Commands
{
    class HeroesInfoCommand : IChatCommand
    {
        #region Members

        private KhadgarBotViewModel _khadgarBotViewModel;
        private List<KeyValuePair<string, string>> _infoRequestList;
        private int _maxRequestCount = 1;

        #endregion

        #region Constructor

        public HeroesInfoCommand(KhadgarBotViewModel khadgarBotViewModel)
        {
            _infoRequestList = new List<KeyValuePair<string, string>>();
            _khadgarBotViewModel = khadgarBotViewModel;
        }

        #endregion

        #region Commands

        //public bool CanProcess(ChatMessage chatMessage)
        //{
        //    return true;// ProcessMessage(chatMessage).Result;
        //}

        public void ProcessMessage(ChatMessage chatMessage)
        {
            var matches = Regex.Matches(chatMessage.Message, @"\[\[((\w+\/\w+)|([\w\s]+))\]\]");
            var results = new List<string>();
            var messageAdded = false;
            _infoRequestList.Clear();
            if(matches.Count > _maxRequestCount)
            {
                //while (!messageAdded)
                //{
                //    messageAdded = _khadgarBotViewModel.AddPriorityWhisperToSendQueue(chatMessage.Username, $"You attempted to request too many results. Max: {_maxRequestCount}");
                //}
                //messageAdded = false;
                _khadgarBotViewModel.AddPriorityWhisperToSendQueue(chatMessage.Username, $"You attempted to request too many results. Max: {_maxRequestCount}");
            }
            else if (matches.Count > 0)
            {
                //while (!messageAdded)
                //{
                //    messageAdded = _khadgarBotViewModel.AddPriorityWhisperToSendQueue(chatMessage.Username, "Working...");
                //}
                //messageAdded = false;
                _khadgarBotViewModel.AddPriorityWhisperToSendQueue(chatMessage.Username, "Working...");

                //each match is one instance of a [[info request]] in a message
                foreach (var match in matches)
                {
                    var input = match.ToString();
                    //the results of one query can, and often will, span multiple responses, so store them in a list
                    results = _khadgarBotViewModel.GetInfoFromHeroesLibrarian(input.Substring(2, input.Length - 4));
                    //for each of the results, set up the list that will be sent back to the requesting user
                    results.ForEach(r => _infoRequestList.Add(new KeyValuePair<string, string>(chatMessage.Username, r)));
                }

                if(_infoRequestList.Count > 0)
                {
                    foreach (var infoReq in _infoRequestList)
                    {
                        //while (!messageAdded)
                        //{
                        //    messageAdded = _khadgarBotViewModel.AddWhisperToSendQueue(infoReq.Key, infoReq.Value);
                        //}
                        //messageAdded = false;
                        _khadgarBotViewModel.AddWhisperToSendQueue(infoReq.Key, infoReq.Value);
                    }

                    _khadgarBotViewModel.AddWhisperToSendQueue(chatMessage.Username, $"Job's done.");
                }
                else
                {
                    //while (!messageAdded)
                    //{
                    //    messageAdded = _khadgarBotViewModel.AddWhisperToSendQueue(chatMessage.Username, "No results.");
                    //}
                    //messageAdded = false;
                    _khadgarBotViewModel.AddWhisperToSendQueue(chatMessage.Username, "No results.");
                }

                //while (!messageAdded)
                //{
                //    messageAdded = _khadgarBotViewModel.AddWhisperToSendQueue(chatMessage.Username, $"Job's done.");
                //}
                //messageAdded = false;
            }
            //if (chatMessage.Message.Substring(0, 2) == "[[" && chatMessage.Message.Substring(chatMessage.Message.Length - 2, 2) == "]]")
            //{
            //    GetHeroesData(chatMessage.Message);
            //    return true;
            //}
        }

        #endregion

        #region Methods

        public void SetMaxRequestCount(int max)
        {
            if (max < 1)
            {
                _maxRequestCount = 1;
            }
            else if(max > 5)
            {
                _maxRequestCount = 5;
            }
            else
            {
                _maxRequestCount = max;
            }
        }

        //private void _infoTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    //im going to need 2 timers for this, one for keeping track of going through multiple commands within one message, and one for spitting out the results list moderated by the global message limit
        //    if(_infoRequestList.Count == 0)
        //    {
        //        return;
        //    }

        //    var temp = _infoRequestList.First();
        //    _khadgarBotViewModel.AddSendWhisperToQueue(temp.Key, temp.Value);
        //    //GetHeroesData(temp.Value.ToString());
        //    _infoRequestList.RemoveAt(0);

        //    if (_infoRequestList.Count == 0)
        //    {
        //        _infoTimer.Enabled = false;
        //    }
        //}

        //private void GetHeroesData(string message)
        //{
        //    var userInput = message.Substring(2, message.Length - 4);
        //    var result = _khadgarBotViewModel.GetInfoFromHeroesLibrarian(userInput);

        //    foreach(var item in result)
        //    {
        //        _khadgarBotViewModel.SendChatMessage(item);
        //        //Thread.Sleep(1000);//boy howdy, do i hate doing this. fortunately its just for testing, but damn, i still hate it
        //    }
        //    //var resultsTimer = new Timer(1000);
        //    //resultsTimer.Elapsed += (sender, e) => ResultsTimer_Elapsed(sender, e, result);
        //    //resultsTimer.Start();
        //}

        //private void ResultsTimer_Elapsed(object sender, ElapsedEventArgs e, List<string> results)
        //{
        //    if (results.FirstOrDefault() != null)
        //    {
        //        _khadgarBotViewModel.SendChatMessage(results.First());
        //        results.RemoveAt(0);
        //    }

        //    if(results.Count > 0)
        //    {

        //    }
        //}

        #endregion
    }
}
