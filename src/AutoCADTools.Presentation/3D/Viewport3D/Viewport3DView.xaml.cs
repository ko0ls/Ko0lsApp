using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation._3D.Viewport3D.Selection;
using AutoCADTools.Service._3D;

namespace AutoCADTools.Presentation._3D.Viewport3D
{
  public partial class Viewport3DView : UserControl
  {
    // ── Win32 P/Invoke ────────────────────────────────────────────
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_MOUSE_LL = 14;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_MOUSEMOVE = 0x0200;

    private static bool IsShiftPressed() => (GetAsyncKeyState(0x10) & 0x8000) != 0;

    // ── Instance state ────────────────────────────────────────────
    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelMouseProc? _mouseProc;
    private Point _lastMousePos;
    private IntPtr _mainWindowHwnd;

    // ── Constructor ───────────────────────────────────────────────
    public Viewport3DView()
    {
      InitializeComponent();
      Loaded += OnLoaded;
      Unloaded += OnUnloaded;
    }

    // ── ViewModel / Service references ───────────────────────────
    private Viewport3DViewModel? _viewModel;
    private SelectionService? _selectionService;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _viewModel = DataContext as Viewport3DViewModel;
      if (_viewModel == null) return;

      // Inject selection service into viewmodel's scene root
      var selServiceField = typeof(Viewport3DViewModel)
        .GetField("_selectionService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      if (selServiceField != null)
        _selectionService = selServiceField.GetValue(_viewModel) as SelectionService;

      // Get the main AutoCAD window handle for coordinate conversion
      var hwnd = Window.GetWindow(MainViewport);
      if (hwnd != null)
        _mainWindowHwnd = new System.Windows.Interop.WindowInteropHelper(hwnd).Handle;

      // Install system-level low-level mouse hook (catches MMB before AutoCAD)
      _mouseProc = MouseHookCallback;
      using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
      using (var curModule = curProcess.MainModule)
      {
        _hookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc,
          GetModuleHandle(curModule!.FileName), 0);
      }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      if (_hookId != IntPtr.Zero)
      {
        UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
      }
    }

    // ── Low-level mouse hook ─────────────────────────────────────
    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0 && _viewModel != null)
      {
        var msg = wParam.ToInt32();

        // lParam points to MOUSEHOOKSTRUCT (or MSLLHOOKSTRUCT on 64-bit)
        // Offset 0 = x, offset 4 = y (both 32-bit ints)
        int x = Marshal.ReadInt32(lParam, 0);
        int y = Marshal.ReadInt32(lParam, 4);
        var pos = new Point(x, y);

        if (msg == WM_MBUTTONDOWN && IsShiftPressed())
        {
          _viewModel.ViewportService.SetOrbitOnMiddle(true);
          _viewModel.ViewportService.CurrentMode = EnumViewportMode.Orbit;
          _lastMousePos = pos;
          return (IntPtr)1; // block from AutoCAD
        }

        if (msg == WM_MBUTTONUP)
        {
          _viewModel.ViewportService.SetOrbitOnMiddle(false);
          return (IntPtr)1; // block from AutoCAD
        }

        // Mouse move during orbit drag
        if (msg == WM_MOUSEMOVE && _viewModel.ViewportService.CurrentMode == EnumViewportMode.Orbit)
        {
          var service = _viewModel.ViewportService;
          var orbitOnMiddleField = typeof(ViewportService)
            .GetField("_orbitOnMiddle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
          var orbitOnMiddle = (bool)(orbitOnMiddleField?.GetValue(service) ?? false);

          if (orbitOnMiddle)
          {
            var deltaX = pos.X - _lastMousePos.X;
            var deltaY = pos.Y - _lastMousePos.Y;
            _lastMousePos = pos;

            if (Math.Abs(deltaX) >= 0.5 || Math.Abs(deltaY) >= 0.5)
            {
              // Convert screen delta to viewport-relative delta using AutoCAD window rect
              var dx = deltaX;
              var dy = deltaY;
              if (_mainWindowHwnd != IntPtr.Zero)
              {
                GetWindowRect(_mainWindowHwnd, out RECT rect);
                var vpSize = MainViewport.RenderSize;
                dx = deltaX / rect.Width * vpSize.Width;
                dy = deltaY / rect.Height * vpSize.Height;
              }
              _viewModel.CameraService.Orbit(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), dx * 0.5);
              _viewModel.CameraService.Orbit(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -dy * 0.5);
            }
          }
        }
      }

      return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
      public int Left, Top, Right, Bottom;
      public double Width => Right - Left;
      public double Height => Bottom - Top;
    }

    // ── WPF mouse events (for LMB/RMB, selection, etc.) ─────────
    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
      if (_viewModel == null) return;
      var pos = e.GetPosition(MainViewport);
      var left = e.LeftButton == MouseButtonState.Pressed;
      var right = e.RightButton == MouseButtonState.Pressed;
      var middle = e.MiddleButton == MouseButtonState.Pressed;
      _viewModel.ViewportService.OnMouseMove(pos, left, right, middle);
    }

    private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_viewModel == null || _selectionService == null) return;
      MainViewport.Focus();

      if (_viewModel.ViewportService.CurrentMode == EnumViewportMode.Select)
      {
        var pos = e.GetPosition(MainViewport);
        _selectionService.PerformHitTest(pos, MainViewport);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
          var topHit = _selectionService.TopHit;
          if (topHit != null)
          {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
              _viewModel.CameraService.ZoomToObjects(new[] { topHit.Object });
            else
            {
              _viewModel.ObjectManager.DeselectAll();
              _viewModel.ObjectManager.Select(topHit.Object.Id);
            }
          }
          else
          {
            _viewModel.ObjectManager.DeselectAll();
          }
        }
      }
    }

    private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
    }

    private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (_viewModel == null) return;
      _viewModel.ViewportService.OnMouseWheel(e.Delta);
    }

    private void Viewport_KeyDown(object sender, KeyEventArgs e)
    {
      if (_viewModel == null) return;
      _viewModel.ViewportService.OnKeyDown(e.Key);
    }

    private void Viewport_KeyUp(object sender, KeyEventArgs e)
    {
      if (_viewModel == null) return;
      _viewModel.ViewportService.OnKeyUp(e.Key);
    }
  }
}
