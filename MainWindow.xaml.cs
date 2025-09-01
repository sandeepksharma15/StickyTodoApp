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
using StickyTodoApp.Models;
using StickyTodoApp.ViewModels;

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

    public TodoViewModel ViewModel { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, __) => PinToDesktop();

        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                var handle = new WindowInteropHelper(this).Handle;
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HTCAPTION, 0);
            }
        };

        DataContext = ViewModel;

        // TEMP: Add sample items for testing
        ViewModel.AddItem(new TodoItem { Title = "Buy milk", Priority = Priority.Urgent });
        ViewModel.AddItem(new TodoItem { Title = "Finish report", Priority = Priority.Normal, DueDate = DateTime.Today.AddDays(2) });
        ViewModel.AddItem(new TodoItem { Title = "Call mom", Priority = Priority.Urgent, DueDate = DateTime.Today });
    }

    private void PinToDesktop()
    {
        var progman = NativeMethods.FindWindow("Progman", null!);

        // Tell Progman to spawn a WorkerW
        NativeMethods.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);

        IntPtr workerw = IntPtr.Zero;
        IntPtr temp = IntPtr.Zero;

        // Find the WorkerW behind desktop icons
        do
        {
            workerw = NativeMethods.FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null!);

            IntPtr shellView = NativeMethods.FindWindowEx(workerw, IntPtr.Zero, "SHELLDLL_DefView", null!);

            if (shellView != IntPtr.Zero)
            {
                temp = NativeMethods.FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null!);
                break;
            }
        } while (workerw != IntPtr.Zero);

        if (temp != IntPtr.Zero)
        {
            var handle = new WindowInteropHelper(this).Handle;
            NativeMethods.SetParent(handle, temp);

            // push it to bottom so normal windows stay above
            NativeMethods.SetWindowPos(handle, NativeMethods.HWND_BOTTOM,
                0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        }
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