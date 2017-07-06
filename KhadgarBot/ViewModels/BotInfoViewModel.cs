using KhadgarBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace KhadgarBot.ViewModels
{
    class BotInfoViewModel : DependencyObject
    {
        #region Members

        #endregion

        #region Constructor

        public BotInfoViewModel()
        {
            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botPass = root.Descendants("pass").First().Value;

            Model = new BotInfo { BotName = botNickname, ChannelName = "ciarenni", OAuth = botPass };
        }

        #endregion

        #region Properties

        public BotInfo Model { get; set; }

        #endregion

        #region Commands

        #endregion

        #region Methods

        #endregion
    }
}
