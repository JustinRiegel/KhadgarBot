using System.ComponentModel.Composition;
using System.Windows.Controls;
using KhadgarBot.ViewModels;

namespace KhadgarBot.Views
{
    /// <summary>
    /// Interaction logic for BotAdminView.xaml
    /// </summary>
    [Export("BotAdminView")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class BotAdminView : UserControl
    {
        public BotAdminView()
        {
            InitializeComponent();
        }

        [Import]
        public BotAdminViewModel ViewModel
        {
            get { return (BotAdminViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
