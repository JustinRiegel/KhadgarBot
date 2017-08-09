using KhadgarBot.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using KhadgarBot.Enums;
using Prism.Commands;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using TwitchLib;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotInfoViewModel : DependencyObject
    {
        #region Members

        private KhadgarBotViewModel _khadgarBotViewModel;
        private ConnectionCredentials _credentials;
        private TwitchClient _client;

        #endregion

        #region Constructor

        [ImportingConstructor]
        public BotInfoViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botPass = root.Descendants("pass").First().Value;

            _khadgarBotViewModel = khadgarBotViewModel;

            Model = new BotInfo(botNickname, botPass, ChannelName);
            LockBotInfo = new DelegateCommand(ExecuteLockBotInfo);
            BotInfoLocked = false;
        }

        #endregion

        #region Properties

        public BotInfo Model { get; set; }

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
                _khadgarBotViewModel.ChangeTabCallback.Execute(TabNameEnum.BotAdmin);
                BotInfoLocked = true;
                Model.ConnectToTwitch();
            }
            else
            {
                Model.LeaveChannel(ChannelName);
            }
        }

        #endregion

        #region Methods

        

        #endregion
    }
}
