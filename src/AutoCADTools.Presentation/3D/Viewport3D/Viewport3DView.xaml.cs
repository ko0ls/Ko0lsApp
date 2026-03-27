using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCADTools.Presentation._3D.Viewport3D.Selection;
using AutoCADTools.Service._3D;

namespace AutoCADTools.Presentation._3D.Viewport3D
{
  public partial class Viewport3DView : UserControl
  {
    public Viewport3DView()
    {
      InitializeComponent();
      Loaded += OnLoaded;
    }

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
    }

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

      // Shift + Middle Mouse Button → temporarily switch to Orbit mode
      if (e.MiddleButton == MouseButtonState.Pressed && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
      {
        _viewModel.ViewportService.CurrentMode = Core._3D.Enums.EnumViewportMode.Orbit;
        return;
      }

      if (_viewModel.ViewportService.CurrentMode == Core._3D.Enums.EnumViewportMode.Select)
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
