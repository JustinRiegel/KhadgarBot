using Prism.Commands;
using System.ComponentModel.Composition;
using System.Windows;
using TwitchLib;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotAdminViewModel : DependencyObject
    {
        #region Constructor

        [ImportingConstructor]
        public BotAdminViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            ConnectToTwitch = new DelegateCommand(ExecuteConnectToTwitch);
        }

        #endregion

        #region Properties

        public DelegateCommand ConnectToTwitch { get; set; }

        public void ExecuteConnectToTwitch()
        {
            
        }

        #endregion

    }
}
