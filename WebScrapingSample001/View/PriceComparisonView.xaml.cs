using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebScrapingSample001.View
{
    /// <summary>
    /// PriceComparisonView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PriceComparisonView : UserControl
    {
        public PriceComparisonView()
        {
            InitializeComponent();

            // 마우스 휠 이벤트 핸들러 추가
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 마우스 휠 이벤트가 발생할 때 스크롤 조작
            if (e.Delta > 0)
            {
                // 스크롤을 올릴 때
                scrollViewer.LineUp();
            }
            else
            {
                // 스크롤을 내릴 때
                scrollViewer.LineDown();
            }

            // 이벤트를 처리한 후 이벤트 전파 중지
            e.Handled = true;
        }
    }
}
