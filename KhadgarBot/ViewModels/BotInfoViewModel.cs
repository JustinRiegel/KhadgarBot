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

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotInfoViewModel : DependencyObject
    {
        #region Members

        private KhadgarBotViewModel _khadgarBotViewModel;

        #endregion

        #region Constructor

        public BotInfoViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botPass = root.Descendants("pass").First().Value;

            _khadgarBotViewModel = khadgarBotViewModel;

            Model = new BotInfo { BotName = botNickname, ChannelName = "ciarenni", OAuth = botPass };
            LockBotInfo = new DelegateCommand(ExecuteLockBotInfo);
            
        }

        #endregion

        #region Properties

        public BotInfo Model { get; set; }

        #endregion

        #region Commands

        public DelegateCommand LockBotInfo { get; set; }

        public void ExecuteLockBotInfo()
        {
            _khadgarBotViewModel.ChangeTabCallback.Execute(TabNameEnum.BotAdmin);
        }

        #endregion

        #region Methods

        #endregion
    }
}
