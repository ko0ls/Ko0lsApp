using AutoCADTools.Presentation.Canvas;
using AutoCADTools.Presentation.Utils;

namespace AutoCADTools.Presentation.ViewModels;

/// <summary>
/// ViewModel for the SwallowFoundation preview canvas, extending CanvasViewModel
/// with a refresh callback invoked after the canvas is loaded so that the drawing
/// is laid out before ZoomToFit runs.
/// </summary>
public class SwallowFoundationPreviewViewModel : CanvasViewModel
{
  private readonly System.Action? _onRefresh;

  public SwallowFoundationPreviewViewModel(System.Action? onRefresh)
  {
    _onRefresh = onRefresh;
    // Replace the default WindowLoadedCommand so we can refresh after base setup
    WindowLoadedCommand = new RelayCommand(() => {
      base.OnWindowLoaded(null);
      _onRefresh?.Invoke();
    });
  }
}
