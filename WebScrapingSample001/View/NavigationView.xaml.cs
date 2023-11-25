using System.Windows.Controls;
using WebScrapingSample001.ViewModel;

namespace WebScrapingSample001.View
{
    /// <summary>
    /// NavigationView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NavigationView : UserControl
    {
        public NavigationView()
        {
            InitializeComponent();
            this.DataContext = new NavigationViewModel();
        }
    }
}
