using KhadgarBot.ViewModels;
using System.Windows.Controls;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for BotAdminView.xaml
    /// </summary>
    public partial class BotAdminView : UserControl
    {
        public BotAdminView()
        {
            InitializeComponent();
            DataContext = new BotAdminViewModel();
        }
    }
}
