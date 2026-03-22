using System.Windows.Input;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas;

public class CanvasViewModel : BindableObject
{
  private bool _isShowDim;
  private System.Windows.Controls.Canvas? _canvas;
  private Wpf.Controls.PanAndZoom.ZoomBorder? _zoomBorder;

  [ChangeTracker]
  public bool IsShowDim
  {
    get => _isShowDim;
    set => SetProperty(ref _isShowDim, value, nameof(IsShowDim));
  }

  public string ShowDimensionLabel => "Command.ShowDimension".GetString();

  public System.Windows.Controls.Canvas? Canvas => _canvas;
  public object? ZoomBorder => _zoomBorder;

  public ICommand MouseMoveCommand { get; }
  public ICommand MouseDownCommand { get; }
  public ICommand MouseUpCommand { get; }
  public ICommand WindowLoadedCommand { get; }
  public ICommand WindowClosedCommand { get; }

  public CanvasViewModel()
  {
    MouseMoveCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseMove);
    MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseDown);
    MouseUpCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseUp);
    WindowLoadedCommand = new RelayCommand(() => OnWindowLoaded(null));
    WindowClosedCommand = new RelayCommand(() => OnWindowClosed(null));
  }

  private void OnWindowLoaded(object? parameter)
  {
    if (parameter is not object[] values || values.Length < 3)
      return;

    if (values[1] is System.Windows.Controls.Canvas canvas) {
      _canvas = canvas;
    }

    if (values[2] is Wpf.Controls.PanAndZoom.ZoomBorder zb) {
      _zoomBorder = zb;
    }

    if (_canvas != null && _zoomBorder != null) {
      UtilsCanvas.ZoomToFit(_canvas, _zoomBorder);
    }
  }

  private void OnWindowClosed(object? parameter)
  {
    _canvas = null;
    _zoomBorder = null;
  }

  private void OnMouseMove(MouseButtonEventArgs? e)
  {
  }

  private void OnMouseDown(MouseButtonEventArgs? e)
  {
  }

  private void OnMouseUp(MouseButtonEventArgs? e)
  {
    if (e is { RightButton: MouseButtonState.Pressed })
      UtilsCanvas.ZoomToFit(e, _canvas, _zoomBorder);
  }
}