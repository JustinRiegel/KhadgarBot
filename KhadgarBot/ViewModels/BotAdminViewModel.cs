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
            HasConnected = false;

            ConnectToTwitch = new DelegateCommand(ExecuteConnectToTwitch);
            JoinChannel = new DelegateCommand(ExecuteJoinChannel);
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

        public bool HasConnected
        {
            get { return (bool)GetValue(HasConnectedProperty); }
            set { SetValue(HasConnectedProperty, value); }
        }

        #region Dependency Properties

        //TODO: temporary until i bind up the khadgarbotModel properly
        private static readonly DependencyProperty BotNameProperty = DependencyProperty.Register("BotName", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));
        private static readonly DependencyProperty OAuthProperty = DependencyProperty.Register("OAuth", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));
        private static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register("ChannelName", typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata("ciarenni"));
        private static readonly DependencyProperty HasConnectedProperty = DependencyProperty.Register("HasConnected", typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        #endregion

        #endregion

        #region Commands

        public DelegateCommand ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            _khadgarBotViewModel.ConnectToTwitch.Invoke();
            HasConnected = true;
        }

        public DelegateCommand JoinChannel { get; set; }

        public void ExecuteJoinChannel()
        {
            _khadgarBotViewModel.JoinChannel.Invoke(ChannelName);
        }

        #endregion

        #region Methods



        #endregion
    }
}
