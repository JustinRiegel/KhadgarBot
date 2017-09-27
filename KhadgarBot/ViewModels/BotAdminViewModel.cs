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
        private KhadgarBotModel _khadgarBotModel;
        private ConnectionCredentials _credentials;

        #endregion

        #region Constructor

        [ImportingConstructor]
        public BotAdminViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            _khadgarBotViewModel = khadgarBotViewModel;
            _khadgarBotModel = _khadgarBotViewModel.Model;

            BotName = _khadgarBotModel.BotName;
            OAuth = _khadgarBotModel.OAuth;
            ChannelName = _khadgarBotModel.ChannelName ?? "ciarenni";
            BotInfoLocked = false;

            LockBotInfo = new DelegateCommand(ExecuteLockBotInfo);
            ConnectToTwitch = new DelegateCommand(ExecuteConnectToTwitch);
            JoinChannel = new DelegateCommand<string>(ExecuteJoinChannel, CanExecuteJoinChannel);
        }

        #endregion

        #region Properties

        public string BotName
        {
            get { return (string)GetValue(BotNameProperty); }
            private set { SetValue(BotNameProperty, value); }
        }

        public string OAuth
        {
            get { return (string)GetValue(OAuthProperty); }
            private set { SetValue(OAuthProperty, value); }
        }

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

        //TODO: temporary until i bind up the khadgarbotModel properly
        public static readonly DependencyProperty BotNameProperty = DependencyProperty.Register("BotName", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty OAuthProperty = DependencyProperty.Register("OAuth", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));

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
                BotInfoLocked = true;
            }
            JoinChannel.RaiseCanExecuteChanged();
        }

        public DelegateCommand ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _khadgarBotModel.Client.OnJoinedChannel += onJoinedChannel;
            _khadgarBotModel.Client.Connect();
        }

        public DelegateCommand<string> JoinChannel { get; set; }

        public bool CanExecuteJoinChannel(string channelName)
        {
            return BotInfoLocked;
        }

        public void ExecuteJoinChannel(string channelName)
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _khadgarBotModel.Client.JoinChannel(channelName);
        }

        public void LeaveChannel(string channelName)
        {
            //TODO: this isn't the right way to name or use this, fix it soon
            _khadgarBotModel.Client.LeaveChannel(channelName);
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                _khadgarBotModel.Client.SendMessage("Hey guys! I am a bot connected via TwitchLib!");
            }));
        }

        #endregion

        #region Methods



        #endregion
    }
}
