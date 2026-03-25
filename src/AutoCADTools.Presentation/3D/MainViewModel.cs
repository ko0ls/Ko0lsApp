using AutoCADTools.Presentation.Canvas;
using AutoCADTools.Presentation.Utils;

namespace AutoCADTools.Presentation._3D
{
  /// <summary>
  /// Root aggregator ViewModel for the dual-view (2D + 3D) host window.
  /// Holds references to all child ViewModels so the TabControl DataContext can reach them.
  /// </summary>
  public class MainViewModel : BindableObject
  {
    public MainViewModel(
      CanvasViewModel canvasViewModel,
      Viewport3DWindowViewModel viewportWindowViewModel)
    {
      CanvasViewModel = canvasViewModel;
      ViewportWindowViewModel = viewportWindowViewModel;
    }

    /// <summary>
    /// 2D Canvas ViewModel — bound to the 2D Canvas tab.
    /// </summary>
    public CanvasViewModel CanvasViewModel { get; }

    /// <summary>
    /// 3D Window ViewModel (which contains all 3 sub-panels) — bound to the 3D tab.
    /// </summary>
    public Viewport3DWindowViewModel ViewportWindowViewModel { get; }
  }
}
