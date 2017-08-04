using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KhadgarBot.Enums;
using Prism.Commands;

namespace KhadgarBot.ViewModels
{
    //possible functionality:
    //death counter, resets on stream death
    //timer, mod level
    public class KhadgarBotViewModel : DependencyObject
    {
        #region Members

        #endregion

        #region Constructor

        public KhadgarBotViewModel()
        {
            BotInfoView = new BotInfoViewModel(this);
            BotAdminView = new BotAdminViewModel(this);
            CommandLogView = new CommandLogViewModel(this);
            ChangeTabCallback = new DelegateCommand<object>(ExecuteChangeTab);
        }

        #endregion

        #region Properties

        public BotInfoViewModel BotInfoView { get; set; }
        public BotAdminViewModel BotAdminView { get; set; }
        public CommandLogViewModel CommandLogView { get; set; }
        
        public TabNameEnum SelectedTabIndex
        {
            get { return (TabNameEnum)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        private static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register("SelectedTabIndex", typeof(TabNameEnum), typeof(KhadgarBotViewModel), new PropertyMetadata(TabNameEnum.BotInfo));

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
