using System.ComponentModel.Composition;
using System.Windows.Controls;
using KhadgarBot.ViewModels;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for BotInfoView.xaml
    /// </summary>
    [Export("BotInfoView")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class BotInfoView : UserControl
    {
        public BotInfoView()
        {
            InitializeComponent();
        }

        [Import]
        public BotInfoViewModel ViewModel
        {
            get { return (BotInfoViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
