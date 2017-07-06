using KhadgarBot.ViewModels;
using System.Windows.Controls;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for CommandLogView.xaml
    /// </summary>
    public partial class CommandLogView : UserControl
    {
        public CommandLogView()
        {
            InitializeComponent();
            DataContext = new CommandLogViewModel();
        }
    }
}
