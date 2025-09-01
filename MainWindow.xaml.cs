using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StickyTodoApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    private const int WM_NCHITTEST = 0x0084;

    public MainWindow()
    {
        InitializeComponent();
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handleSource = (HwndSource)PresentationSource.FromVisual(this);
        handleSource.AddHook(WindowProc);
    }

    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCHITTEST)
        {
            handled = true;
            Point pos = PointFromScreen(new Point((int)lParam & 0xFFFF, (int)lParam >> 16));
            double resizeBorder = 8; // thickness of resize area

            if (pos.Y <= resizeBorder)
            {
                if (pos.X <= resizeBorder) return (IntPtr)HTTOPLEFT;
                if (pos.X >= ActualWidth - resizeBorder) return (IntPtr)HTTOPRIGHT;
                return (IntPtr)HTTOP;
            }
            else if (pos.Y >= ActualHeight - resizeBorder)
            {
                if (pos.X <= resizeBorder) return (IntPtr)HTBOTTOMLEFT;
                if (pos.X >= ActualWidth - resizeBorder) return (IntPtr)HTBOTTOMRIGHT;
                return (IntPtr)HTBOTTOM;
            }
            else if (pos.X <= resizeBorder)
            {
                return (IntPtr)HTLEFT;
            }
            else if (pos.X >= ActualWidth - resizeBorder)
            {
                return (IntPtr)HTRIGHT;
            }
        }
        return IntPtr.Zero;
    }
}