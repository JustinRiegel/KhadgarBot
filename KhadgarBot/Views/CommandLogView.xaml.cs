using System.ComponentModel.Composition;
using KhadgarBot.ViewModels;
using System.Windows.Controls;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for CommandLogView.xaml
    /// </summary>
    [Export("CommandLogView")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CommandLogView : UserControl
    {
        public CommandLogView()
        {
            InitializeComponent();
        }

        [Import]
        public CommandLogViewModel ViewModel
        {
            get { return (CommandLogViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
