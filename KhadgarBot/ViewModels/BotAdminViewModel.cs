using KhadgarBot.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using TwitchLib;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotAdminViewModel : DependencyObject
    {
        #region Members

        private KhadgarBotViewModel _khadgarBotViewModel;
        private ConnectionCredentials _credentials;
        //TODO: this isn't the right way to name or use this, fix it soon
        private TwitchClient _client;

        #endregion

        #region Constructor

        [ImportingConstructor]
        public BotAdminViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
            //TODO: this isn't the right way to name or use this, fix it soon
            _client = _khadgarBotViewModel.Client;
            BotInfoLocked = false;

            LockBotInfo = new DelegateCommand(ExecuteLockBotInfo);
            ConnectToTwitch = new DelegateCommand(ExecuteConnectToTwitch);
        }

        #endregion

        #region Properties

        public string ChannelName
        {
            get { return (string)GetValue(ChannelNameProperty); }
            set { SetValue(ChannelNameProperty, value); }
        }

        public bool BotInfoLocked
        {
            get { return (bool)GetValue(BotInfoLockedProperty); }
            set { SetValue(BotInfoLockedProperty, value); }
        }

        #region Dependency Properties

        private static readonly DependencyProperty ChannelNameProperty =
            DependencyProperty.Register("ChannelName", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata("ciarenni"));

        private static readonly DependencyProperty BotInfoLockedProperty =
            DependencyProperty.Register("BotInfoLocked", typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        #endregion

        #endregion

        #region Commands

        public DelegateCommand LockBotInfo { get; set; }

        public void ExecuteLockBotInfo()
        {
            if (!BotInfoLocked)
            {
                //_khadgarBotViewModel.ChangeTabCallback.Execute(TabNameEnum.BotAdmin);
                BotInfoLocked = true;
                JoinChannel(ChannelName);
            }
            else
            {
                LeaveChannel(ChannelName);
            }
        }

        public DelegateCommand ConnectToTwitch { get; set; }

        //TODO: implement a button for connecting to twitch for testing
        public void ExecuteConnectToTwitch()
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _client.OnJoinedChannel += onJoinedChannel;
            _client.Connect();
        }

        public void JoinChannel(string channelName)
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _client.JoinChannel(channelName);
        }

        public void LeaveChannel(string channelName)
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _client.LeaveChannel(channelName);
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            Dispatcher.Invoke(new Action(() => {
                _client.SendMessage("Hey guys! I am a bot connected via TwitchLib!");
            }));
        }

        #endregion

        #region Methods



        #endregion
    }
}
