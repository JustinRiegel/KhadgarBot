using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace KhadgarBot.Models
{
    public class BotInfo : DependencyObject
    {
        #region Members

        #endregion

        #region Constructor

        public BotInfo(string botNickname, string botPass, string channelName)
        {
            BotName = botNickname;
            OAuth = botPass;
            ChannelName = channelName;
        }

        #endregion

        #region Properties

        public string BotName
        {
            get { return (string)GetValue(BotNameProperty); }
            set { SetValue(BotNameProperty, value); }
        }

        public string OAuth
        {
            get { return (string)GetValue(OAuthProperty); }
            set { SetValue(OAuthProperty, value); }
        }

        public string ChannelName
        {
            get { return (string)GetValue(ChannelNameProperty); }
            set { SetValue(ChannelNameProperty, value); }
        }

        #region DependencyProperties

        public static readonly DependencyProperty BotNameProperty = DependencyProperty.Register("BotName", typeof(string), typeof(BotInfo), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty OAuthProperty = DependencyProperty.Register("OAuth", typeof(string), typeof(BotInfo), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register("ChannelName", typeof(string), typeof(BotInfo), new PropertyMetadata(default(string)));

        #endregion

        #endregion

        
    }
}
