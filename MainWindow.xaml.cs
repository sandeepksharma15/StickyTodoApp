using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // added for TextBoxBase
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
    public IEnumerable<Priority> PriorityValues => Enum.GetValues(typeof(Priority)).Cast<Priority>();

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, __) =>
        {
            PinToDesktop();
            Activate(); // ensure focus after pinning
        };

        // Allow dragging only when clicking on non-interactive background elements
        MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

        DataContext = ViewModel;

        // TEMP: Add sample items for testing
        ViewModel.AddItem(new TodoItem { Title = "Buy milk", Priority = Priority.Urgent });
        ViewModel.AddItem(new TodoItem { Title = "Finish report", Priority = Priority.Normal, DueDate = DateTime.Today.AddDays(2) });
        ViewModel.AddItem(new TodoItem { Title = "Call mom", Priority = Priority.Urgent, DueDate = DateTime.Today });
    }

    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            var origin = e.OriginalSource as DependencyObject;
            if (!IsInteractiveElement(origin))
            {
                try { DragMove(); } catch { /* ignore */ }
            }
        }
    }

    private static bool IsInteractiveElement(DependencyObject? d)
    {
        while (d != null)
        {
            if (d is TextBoxBase || d is PasswordBox || d is ComboBox || d is DatePicker || d is Button || d is CheckBox || d is Slider || d is ListBox || d is ListView || d is ItemsControl)
                return true;
            d = VisualTreeHelper.GetParent(d);
        }
        return false;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (ViewModel.AddCommand.CanExecute(null))
                ViewModel.AddCommand.Execute(null);
        }
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

            // Keep size/pos but allow activation
            NativeMethods.SetWindowPos(handle, NativeMethods.HWND_BOTTOM,
                0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (PresentationSource.FromVisual(this) is HwndSource handleSource)
        {
            handleSource.AddHook(WindowProc);
        }
    }

    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCHITTEST)
        {
            // Use proper signed extraction of coordinates
            int x = (short)((long)lParam & 0xFFFF);
            int y = (short)(((long)lParam >> 16) & 0xFFFF);
            Point pos = PointFromScreen(new Point(x, y));
            double resizeBorder = 8;

            bool left = pos.X <= resizeBorder;
            bool right = pos.X >= ActualWidth - resizeBorder;
            bool top = pos.Y <= resizeBorder;
            bool bottom = pos.Y >= ActualHeight - resizeBorder;

            if (top && left) { handled = true; return (IntPtr)HTTOPLEFT; }
            if (top && right) { handled = true; return (IntPtr)HTTOPRIGHT; }
            if (bottom && left) { handled = true; return (IntPtr)HTBOTTOMLEFT; }
            if (bottom && right) { handled = true; return (IntPtr)HTBOTTOMRIGHT; }
            if (top) { handled = true; return (IntPtr)HTTOP; }
            if (bottom) { handled = true; return (IntPtr)HTBOTTOM; }
            if (left) { handled = true; return (IntPtr)HTLEFT; }
            if (right) { handled = true; return (IntPtr)HTRIGHT; }
            // For interior let default processing occur (do NOT set handled)
        }
        return IntPtr.Zero;
    }
}