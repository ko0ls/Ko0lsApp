using System.Windows.Input;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;
using AutoCADTools.Presentation.Canvas.Utils;
using Wpf.Controls.PanAndZoom;

namespace AutoCADTools.Presentation.Canvas;

public class CanvasViewModel : BindableObject
{
  private bool _isShowDim = true;
  private System.Windows.Controls.Canvas? _canvas;
  private Wpf.Controls.PanAndZoom.ZoomBorder? _zoomBorder;

  [ChangeTracker]
  public bool IsShowDim
  {
    get => _isShowDim;
    set => SetProperty(ref _isShowDim, value, nameof(IsShowDim));
  }


  public System.Windows.Controls.Canvas? Canvas => _canvas;
  public ZoomBorder? ZoomBorder => _zoomBorder;

  public ICommand MouseMoveCommand { get; }
  public ICommand MouseDownCommand { get; }
  public ICommand MouseUpCommand { get; }
  public ICommand WindowLoadedCommand { get; protected set; }
  public ICommand WindowClosedCommand { get; }
  public ICommand WindowSizeChangedCommand { get; }

  public CanvasViewModel()
  {
    MouseMoveCommand = new RelayCommand<MouseEventArgs>(null, OnMouseMove);
    MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseDown);
    MouseUpCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseUp);
    WindowLoadedCommand = new RelayCommand<object[]>(null, OnWindowLoaded);
    WindowClosedCommand = new RelayCommand(() => OnWindowClosed(null));
    WindowSizeChangedCommand = new RelayCommand<object>(
      _ => true,
      _ => {
        if (_canvas != null && _zoomBorder != null)
          UtilsCanvas.ZoomToFit(_canvas, _zoomBorder);
      });
  }

  protected virtual void OnWindowLoaded(object? parameter)
  {
    if (parameter is not object[] values || values.Length < 3)
      return;

    if (values[1] is System.Windows.Controls.Canvas canvas) {
      _canvas = canvas;
    }

    if (values[2] is Wpf.Controls.PanAndZoom.ZoomBorder zb) {
      _zoomBorder = zb;
    }

    if (_canvas != null && _zoomBorder != null)
      UtilsCanvas.ZoomToFit(_canvas, _zoomBorder);
  }

  private void OnWindowClosed(object? parameter)
  {
    _canvas = null;
    _zoomBorder = null;
  }

  private void OnMouseMove(MouseEventArgs? e)
  {
  }

  private void OnMouseDown(MouseButtonEventArgs? e)
  {
    if ( e is { ChangedButton: MouseButton.Middle, ClickCount: 2 } ) {
      UtilsCanvas.ZoomToFit( e, _canvas, _zoomBorder ) ;
    }
  }

  private void OnMouseUp(MouseButtonEventArgs? e)
  {
  }
}
