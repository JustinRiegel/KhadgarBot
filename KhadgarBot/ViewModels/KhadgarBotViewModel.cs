using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;
using System.ComponentModel.Composition;
using KhadgarBot.Models;
using System.Linq;
using System.Xml.Linq;

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
            BotAdminViewModel = new BotAdminViewModel(this);
            CommandLogViewModel = new CommandLogViewModel(this);
            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);
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

        private static readonly DependencyProperty SelectedTabIndexProperty = DependencyProperty.Register("SelectedTabIndex", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotAdmin));

        #endregion

        #region Commands

        public DelegateCommand<object> ChangeTabCallback { get; set; }

        public void ExecuteChangeTab(object selectedIndex)
        {
            SelectedTabIndex = (TabNameEnum)selectedIndex;
        }

        #endregion
    }
}
