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

        private List<ChatMessage> _messages = new List<ChatMessage>();

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

        //public bool CanExecuteJoinChannel(string channelName)
        //{
        //    return ConnectedToTwitch;
        //}

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
            //Model.Client.OnJoinedChannel -= onJoinedChannel;
            Dispatcher.Invoke(new Action(() => {
                Model.Client.OnJoinedChannel -= onJoinedChannel;
                Model.Client.SendMessage("Knowledge is power.");
            }));
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            //DisplayName
            //Username
            //Message
            //Channel
            _messages.Add(e.ChatMessage);
        }

        #endregion
    }
}
