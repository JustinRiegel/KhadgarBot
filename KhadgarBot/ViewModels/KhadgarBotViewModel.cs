using System;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using System.ComponentModel.Composition;
using KhadgarBot.Models;
using System.Linq;
using System.Xml.Linq;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using System.Collections.Generic;
using System.Timers;

namespace KhadgarBot.ViewModels
{
    //possible functionality:
    //death counter, resets on stream death
    //timer, mod level
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class KhadgarBotViewModel : DependencyObject
    {
        #region Members

        private const int _chatPollTimerValue = 30000;

        private List<ChatMessage> _messages = new List<ChatMessage>();
        private Dictionary<string, int> _chatPollEntries = new Dictionary<string, int>();
        private Timer _chatPollTimer = new Timer(_chatPollTimerValue);
        private bool _chatPollTimerIsRunning = false;

        #endregion

        #region Constructor

        [ImportingConstructor]
        public KhadgarBotViewModel()
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botOAuth = root.Descendants("pass").First().Value;

            Model = new KhadgarBotModel(botNickname, botOAuth);
            ConnectedToTwitch = false;

            BotAdminViewModel = new BotAdminViewModel(this);
            CommandLogViewModel = new CommandLogViewModel(this);

            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);
            ConnectToTwitch = new Action(ExecuteConnectToTwitch);
            JoinChannel = new Action<string>(ExecuteJoinChannel);
            _chatPollTimer.Elapsed += onChatPollTimerElapsed;
        }

        #endregion

        #region Properties

        public KhadgarBotModel Model { get; set; }
        public BotAdminViewModel BotAdminViewModel { get; set; }
        public CommandLogViewModel CommandLogViewModel { get; set; }

        public TabNameEnum SelectedTabIndex
        {
            get { return (TabNameEnum)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        public bool ConnectedToTwitch
        {
            get { return (bool)GetValue(ConnectedToTwitchProperty); }
            set { SetValue(ConnectedToTwitchProperty, value); }
        }

        private static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register("SelectedTabIndex", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotAdmin));

        private static readonly DependencyProperty ConnectedToTwitchProperty =
            DependencyProperty.Register("ConnectedToTwitch", typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        #endregion

        #region Commands

        public DelegateCommand<object> ChangeTabCallback { get; set; }

        public void ExecuteChangeTab(object selectedIndex)
        {
            SelectedTabIndex = (TabNameEnum)selectedIndex;
        }

        public Action ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            Model.Client.OnJoinedChannel += onJoinedChannel;
            Model.Client.Connect();
            ConnectedToTwitch = true;
        }

        public Action<string> JoinChannel { get; set; }

        public void ExecuteJoinChannel(string channelName)
        {
            Model.Client.JoinChannel(channelName);
            Model.Client.OnMessageReceived += onMessageReceived;
        }

        public void ExecuteLeaveChannel(string channelName)
        {
            Model.Client.LeaveChannel(channelName);
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                Model.Client.OnJoinedChannel -= onJoinedChannel;
                Model.Client.SendMessage("Knowledge is power.");
            }));
        }

        private void onChatPollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _chatPollTimerIsRunning = false;
            _chatPollTimer.Stop();
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
                result = result.Substring(0, result.Length - 3);
                Dispatcher.Invoke(new Action(() =>
                {
                    Model.Client.SendMessage(String.Format("There was a tie! The winners of the poll are {0} with {1} votes!", result, maxVotes));
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Model.Client.SendMessage(String.Format("The winner of the poll is {0} with {1} votes!", winners.First().Key, maxVotes));
                }));
            }
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            //DisplayName
            //Username
            //Message
            //Channel
            //_messages.Add(e.ChatMessage);

            var chatMessage = e.ChatMessage;

            if(_chatPollTimerIsRunning)
            {
                CheckMessageForChatPollFormatting(chatMessage.Channel, chatMessage.Username, chatMessage.Message);
            }

            if((chatMessage.IsModerator || chatMessage.IsBroadcaster) && chatMessage.Message[0] =='!')
            {
                if(chatMessage.Message == "!chatpoll")
                {
                    _chatPollTimer.Start();
                    _chatPollTimerIsRunning = true;
                    _chatPollEntries.Clear();
                    Dispatcher.Invoke(new Action(() => {
                        Model.Client.SendMessage("The streamer has asked for a poll. Entries will be accepted for the next " + (_chatPollTimerValue / 1000).ToString() + " seconds.");
                    }));
                }
            }
        }

        #endregion

        #region Methods

        private void CheckMessageForChatPollFormatting(string channel, string username, string message)
        {
            if (message.Length > 1)
                return;

            if(Int32.TryParse(message, out int vote))
            {
                if(!_chatPollEntries.ContainsKey(username))
                {
                    _chatPollEntries.Add(username, vote);
                }
            }
        }

        #endregion
    }
}
