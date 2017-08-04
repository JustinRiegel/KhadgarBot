using System.ComponentModel.Composition;
using System.Windows;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BotAdminViewModel : DependencyObject
    {
        public BotAdminViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            
        }
    }
}
