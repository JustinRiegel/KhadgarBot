using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KhadgarBot.ViewModels
{
    //possible functionality:
    //death counter, resets on stream death
    //timer, mod level
    class KhadgarBotViewModel : DependencyObject
    {
        #region Members
        #endregion

        #region Constructor

        public KhadgarBotViewModel()
        {
            BotInfoView = new BotInfoViewModel();
            BotAdminView = new BotAdminViewModel();
            CommandLogView = new CommandLogViewModel();
        }

        #endregion

        #region Properties

        public BotInfoViewModel BotInfoView { get; set; }
        public BotAdminViewModel BotAdminView { get; set; }
        public CommandLogViewModel CommandLogView { get; set; }

        #endregion
    }
}
