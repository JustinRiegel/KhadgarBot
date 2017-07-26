using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KhadgarBot.Enums;

namespace KhadgarBot.ViewModels
{
    //possible functionality:
    //death counter, resets on stream death
    //timer, mod level
    class KhadgarBotViewModel : DependencyObject
    {
        #region Members

        private TabNameEnum TabName = new TabNameEnum();

        #endregion

        #region Constructor

        public KhadgarBotViewModel()
        {
            BotInfoView = new BotInfoViewModel();
            BotAdminView = new BotAdminViewModel();
            CommandLogView = new CommandLogViewModel();
            ChangeTabCallback = new DelegateCommand<TabNameEnum>(ExecuteChangeTab);
        }

        #endregion

        #region Properties

        public BotInfoViewModel BotInfoView { get; set; }
        public BotAdminViewModel BotAdminView { get; set; }
        public CommandLogViewModel CommandLogView { get; set; }
        
        public TabNameEnum SelectedTabName
        {
            get { return (TabNameEnum)GetValue(SelectedTabNameProperty); }
            set { SetValue(SelectedTabNameProperty, value); }
        }

        private static readonly DependencyProperty SelectedTabNameProperty =
            DependencyProperty.Register("SelectedTabName", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotInfo));

        #endregion

        #region Commands

        public DelegateCommand<TabNameEnum> ChangeTabCallback { get; set; }

        public void ExecuteChangeTab(TabNameEnum tabName)
        {
            SelectedTabName = tabName;
        }

        #endregion
    }
}
