using KhadgarBot.Models;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using Prism.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TwitchLib.Client.Models;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotAdminViewModel : DependencyObject, INotifyPropertyChanged
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
            ChannelName = _khadgarBotModel.ChannelName ?? "raysfire";//"ciarenni";
            HasConnected = false;
            HasConnectedButNotJoined = false;
            HasJoined = false;

            ConnectToTwitch = new DelegateCommand(ExecuteConnectToTwitch);
            JoinChannel = new DelegateCommand(ExecuteJoinChannel);
            LeaveChannel = new DelegateCommand(ExecuteLeaveChannel);
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
            get => (bool)GetValue(HasConnectedProperty);
            set {
                SetValue(HasConnectedProperty, value);
                NotifyPropertyChanged(nameof(HasConnected));
            }
        }

        public bool HasConnectedButNotJoined
        {
            get => (bool)GetValue(HasConnectedButNotJoinedProperty);
            set
            {
                SetValue(HasConnectedButNotJoinedProperty, value);
                NotifyPropertyChanged(nameof(HasConnectedButNotJoined));
            }
        }

        public bool HasJoined
        {
            get => (bool)GetValue(HasJoinedProperty);
            set
            {
                SetValue(HasJoinedProperty, value);
                NotifyPropertyChanged(nameof(HasJoined));
            }
        }

        #region Dependency Properties

        private static readonly DependencyProperty BotNameProperty = DependencyProperty.Register(nameof(BotName), typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));
        private static readonly DependencyProperty OAuthProperty = DependencyProperty.Register(nameof(OAuth), typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata(default(string)));
        private static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register(nameof(ChannelName), typeof(string), typeof(KhadgarBotViewModel), new PropertyMetadata("ciarenni"));
        private static readonly DependencyProperty HasConnectedProperty = DependencyProperty.Register(nameof(HasConnected), typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));
        private static readonly DependencyProperty HasConnectedButNotJoinedProperty = DependencyProperty.Register(nameof(HasConnectedButNotJoined), typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));
        private static readonly DependencyProperty HasJoinedProperty = DependencyProperty.Register(nameof(HasJoined), typeof(bool), typeof(KhadgarBotViewModel), new PropertyMetadata(false));

        #endregion

        #endregion

        #region Commands

        public DelegateCommand ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            //assuming the connect succeeds is bad, look into if the TwitchLib provides info on connection success
            _khadgarBotViewModel.ConnectToTwitch.Invoke();
            HasConnected = true;
            HasConnectedButNotJoined = true;
        }

        public DelegateCommand JoinChannel { get; set; }

        public void ExecuteJoinChannel()
        {
            //assuming the join succeeds is bad, look into if the TwitchLib provides info on join success
            _khadgarBotViewModel.JoinChannel.Invoke(ChannelName);
            HasConnectedButNotJoined = false;
            HasJoined = true;
        }

        public DelegateCommand LeaveChannel { get; set; }

        public void ExecuteLeaveChannel()
        {
            //assuming the join succeeds is bad, look into if the TwitchLib provides info on join success
            _khadgarBotViewModel.LeaveChannel.Invoke(ChannelName);
            HasConnectedButNotJoined = true;
            HasJoined = false;
        }

        #endregion

        #region Methods
        


        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
