using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Image_Document_Converter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void WindowMinimizingEvent(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void WindowMaximizedEvent(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
        else this.WindowState = WindowState.Maximized;
    }

    private void WindowCloseEvent(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TitleBarMouseLeftDownEvent(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
            else this.WindowState = WindowState.Maximized;
        }
        else
        {
            this.DragMove();
        }
    }

    private void Splitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        // 우측 미리보기 창의 너비를 조절 
        // (왼쪽으로 드래그하면 e.HorizontalChange가 음수이므로 너비가 커짐)
        double newWidth = PreviewColumn.ActualWidth - e.HorizontalChange;

        // MinWidth 제약 조건 강제 적용 (오버슈팅 방지)
        if (newWidth < PreviewColumn.MinWidth)
        {
            newWidth = PreviewColumn.MinWidth;
        }

        // 너무 넓어져서 왼쪽 목록이 사라지는 것을 방지하기 위한 최대 크기 제한 (선택 사항)
        double maxAllowedWidth = this.ActualWidth - 150 - 5; // 창 너비 - 좌측 최소너비 - 스플리터 너비
        if (newWidth > maxAllowedWidth && maxAllowedWidth > 0)
        {
            newWidth = maxAllowedWidth;
        }

        // 최종 크기를 픽셀 단위로 적용
        PreviewColumn.Width = new GridLength(newWidth, GridUnitType.Pixel);
    }

    private void Grid_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }
}