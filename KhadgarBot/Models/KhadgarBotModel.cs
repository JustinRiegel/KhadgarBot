using System.Windows;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace KhadgarBot.Models
{

    public class KhadgarBotModel : DependencyObject
    {
        #region Constructor

        public KhadgarBotModel(string botNickname, string botOAuth)
        {
            BotName = botNickname;
            OAuth = botOAuth;
            Credentials = new ConnectionCredentials(botNickname, botOAuth);
            Client = new TwitchClient();
            Client.Initialize(Credentials);
        }

        #endregion

        #region Properties

        public TwitchClient Client
        {
            get { return (TwitchClient)GetValue(ClientProperty); }
            private set { SetValue(ClientProperty, value); }
        }

        public ConnectionCredentials Credentials
        {
            get { return (ConnectionCredentials)GetValue(CredentialsProperty); }
            private set { SetValue(CredentialsProperty, value); }
        }

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
            private set { SetValue(ChannelNameProperty, value); }
        }

        #region DependencyProperties

        public static readonly DependencyProperty BotNameProperty = DependencyProperty.Register(nameof(BotName), typeof(string), typeof(KhadgarBotModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty OAuthProperty = DependencyProperty.Register(nameof(OAuth), typeof(string), typeof(KhadgarBotModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register(nameof(ChannelName), typeof(string), typeof(KhadgarBotModel), new PropertyMetadata(default(string)));
        private static readonly DependencyProperty ClientProperty = DependencyProperty.Register(nameof(Client), typeof(TwitchClient), typeof(KhadgarBotModel));
        //private static readonly DependencyProperty TwitchAPIProperty = DependencyProperty.Register(nameof(TwitchAPI), typeof(TwitchAPI), typeof(KhadgarBotModel));
        private static readonly DependencyProperty CredentialsProperty = DependencyProperty.Register(nameof(Credentials), typeof(ConnectionCredentials), typeof(KhadgarBotModel));

        #endregion

        #endregion

        #region Methods

        public void SetChannelName(string channelName)
        {
            ChannelName = channelName;
        }

        #endregion
    }
}
