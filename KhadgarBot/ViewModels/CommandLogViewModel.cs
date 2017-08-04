using System.ComponentModel.Composition;
using System.Windows;

namespace KhadgarBot.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CommandLogViewModel : DependencyObject
    {
        public CommandLogViewModel(KhadgarBotViewModel khadgarBotViewModel)
        {
            
        }
    }
}
