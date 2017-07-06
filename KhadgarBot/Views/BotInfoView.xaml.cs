using System.Windows.Controls;
using KhadgarBot.ViewModels;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for BotInfoView.xaml
    /// </summary>
    public partial class BotInfoView : UserControl
    {
        public BotInfoView()
        {
            InitializeComponent();
            DataContext = new BotInfoViewModel();
        }
    }
}
