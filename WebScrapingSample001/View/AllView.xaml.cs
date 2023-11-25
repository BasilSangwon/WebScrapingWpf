using System.Windows.Controls;

namespace WebScrapingSample001.View
{
    /// <summary>
    /// AllView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AllView : UserControl
    {
        public AllView()
        {
            InitializeComponent();

            // 마우스 휠 이벤트 핸들러 추가
            sv_danawaPI.PreviewMouseWheel += Sv_danawaPI_PreviewMouseWheel;
            sv_naverShoppingPI.PreviewMouseWheel += Sv_naverShoppingPI_PreviewMouseWheel;
        }

       

        private void Sv_danawaPI_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // 마우스 휠 이벤트가 발생할 때 스크롤 조작
            if (e.Delta > 0)
            {
                // 스크롤을 올릴 때
                sv_danawaPI.LineUp();
            }
            else
            {
                // 스크롤을 내릴 때
                sv_danawaPI.LineDown();
            }

            // 이벤트를 처리한 후 이벤트 전파 중지
            e.Handled = true;
        }

        private void Sv_naverShoppingPI_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // 마우스 휠 이벤트가 발생할 때 스크롤 조작
            if (e.Delta > 0)
            {
                // 스크롤을 올릴 때
                sv_naverShoppingPI.LineUp();
            }
            else
            {
                // 스크롤을 내릴 때
                sv_naverShoppingPI.LineDown();
            }

            // 이벤트를 처리한 후 이벤트 전파 중지
            e.Handled = true;
        }
    }
}
