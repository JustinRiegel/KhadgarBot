using KhadgarBot.ViewModels;
using System.Windows.Controls;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for KhadgarBotView.xaml
    /// </summary>
    public partial class KhadgarBotView : UserControl
    {
        public KhadgarBotView()
        {
            InitializeComponent();
            DataContext = new KhadgarBotViewModel();
        }
    }
}
