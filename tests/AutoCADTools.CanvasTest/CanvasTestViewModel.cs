using System.Windows.Input;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Canvas.Utils;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Annotation;

namespace AutoCADTools.CanvasTest;

public class CanvasTestViewModel : BindableObject
{
  private System.Windows.Controls.Canvas? _canvas;
  private Wpf.Controls.PanAndZoom.ZoomBorder? _zoomBorder;
  private List<ShapeBase> _allShapes = new List<ShapeBase>();
  private List<AnnotationBase> _allAnnotations = new List<AnnotationBase>();

  private double _xValue;
  private double _yValue;
  private double _zoomX;
  private double _zoomY;
  private double _offsetX;
  private double _offsetY;

  private List<ScaleModel> _allScales = new List<ScaleModel>();
  private ScaleModel? _selectedScale;

  public double XValue
  {
    get => _xValue;
    set => SetProperty(ref _xValue, value, nameof(XValue));
  }

  public double YValue
  {
    get => _yValue;
    set => SetProperty(ref _yValue, value, nameof(YValue));
  }

  public double ZoomX
  {
    get => _zoomX;
    set => SetProperty(ref _zoomX, value, nameof(ZoomX));
  }

  public double ZoomY
  {
    get => _zoomY;
    set => SetProperty(ref _zoomY, value, nameof(ZoomY));
  }

  public double OffsetX
  {
    get => _offsetX;
    set => SetProperty(ref _offsetX, value, nameof(OffsetX));
  }

  public double OffsetY
  {
    get => _offsetY;
    set => SetProperty(ref _offsetY, value, nameof(OffsetY));
  }

  public List<ScaleModel> AllScales
  {
    get => _allScales;
    set => SetProperty(ref _allScales, value, nameof(AllScales));
  }

  public ScaleModel? SelectedScale
  {
    get => _selectedScale;
    set
    {
      if (SetProperty(ref _selectedScale, value, nameof(SelectedScale)))
        OnScaleChanged();
    }
  }

  public ICommand WindowLoadedCommand { get; }
  public ICommand ZoomToFitCommand { get; }
  public ICommand MouseMoveCommand { get; }
  public ICommand MouseDownCommand { get; }

  public CanvasTestViewModel()
  {
    AllScales.Add(new ScaleModel(10));
    AllScales.Add(new ScaleModel(20));
    AllScales.Add(new ScaleModel(50));
    AllScales.Add(new ScaleModel(100));
    AllScales.Add(new ScaleModel(200));

    WindowLoadedCommand = new RelayCommand<object?>(null, OnWindowLoaded);
    ZoomToFitCommand = new RelayCommand(OnZoomToFit);
    MouseMoveCommand = new RelayCommand<MouseEventArgs>(null, OnMouseMove);
    MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(null, OnMouseDown);
  }

  private void OnWindowLoaded(object? parameter)
  {
    if (parameter is not object[] values || values.Length < 3)
      return;

    if (values[1] is System.Windows.Controls.Canvas canvas)
      _canvas = canvas;

    if (values[2] is Wpf.Controls.PanAndZoom.ZoomBorder zoomBorder)
      _zoomBorder = zoomBorder;

    if (_canvas != null && _zoomBorder != null)
    {
      DrawCanvas();
      UtilsCanvas.ZoomToFit(_canvas, _zoomBorder);
    }
  }

  private void OnZoomToFit()
  {
    if (_canvas != null && _zoomBorder != null)
      UtilsCanvas.ZoomToFit(_canvas, _zoomBorder);
  }

  private void OnMouseMove(MouseEventArgs? e)
  {
    if (_canvas == null || _zoomBorder == null)
      return;

    var point = e?.GetPosition(_canvas) ?? new System.Windows.Point(0, 0);
    XValue = point.X;
    YValue = point.Y;
    ZoomX = _zoomBorder.ZoomX;
    ZoomY = _zoomBorder.ZoomY;
    OffsetX = _zoomBorder.OffsetX;
    OffsetY = _zoomBorder.OffsetY;

    foreach (var shape in _allShapes)
      shape.IsMoveOver = false;

    foreach (var shape in _allShapes)
    {
      if (shape.CheckMoveOver(point))
      {
        shape.IsMoveOver = true;
        break;
      }
    }

    foreach (var annotation in _allAnnotations)
      annotation.IsMoveOver = false;

    foreach (var annotation in _allAnnotations)
    {
      if (annotation.CheckMoveOver(point))
      {
        annotation.IsMoveOver = true;
        break;
      }
    }
  }

  private void OnMouseDown(MouseButtonEventArgs? e)
  {
    if (_canvas == null)
      return;

    if (e?.ClickCount == 2 && e.ChangedButton == MouseButton.Middle)
    {
      UtilsCanvas.ZoomToFit(e, _canvas, _zoomBorder);
      return;
    }

    var point = e?.GetPosition(_canvas) ?? new System.Windows.Point(0, 0);

    if (e?.ChangedButton == MouseButton.Left)
    {
      foreach (var shape in _allShapes)
        shape.IsSelected = shape.CheckMoveOver(point);

      foreach (var annotation in _allAnnotations)
        annotation.IsSelected = annotation.CheckMoveOver(point);
    }
  }

  private void OnScaleChanged()
  {
    if (_canvas == null)
      return;

    _canvas.Children.Clear();
    _allShapes.Clear();
    _allAnnotations.Clear();
    DrawCanvas();
  }

  private void DrawCanvas()
  {
    if (_canvas == null || SelectedScale == null)
      return;

    double scale = SelectedScale.Scale;

    // Cross hair lines
    var lineH = new Line2D(_canvas, new System.Windows.Point(0, 0), new System.Windows.Point(1000, 0), EnumLineType.Solid, 2, System.Windows.Media.Brushes.Black);
    var lineV = new Line2D(_canvas, new System.Windows.Point(0, 0), new System.Windows.Point(0, 1000), EnumLineType.Solid, 2, System.Windows.Media.Brushes.Black);
    _allShapes.Add(lineH);
    _allShapes.Add(lineV);

    // Diagonal line
    var lineA = new Line2D(_canvas, new System.Windows.Point(0, 0), new System.Windows.Point(500, -500), EnumLineType.Solid, 2, System.Windows.Media.Brushes.Black);
    _allShapes.Add(lineA);

    // Angular annotation (L-shape angle)
    var p1 = new System.Windows.Point(0, 0);
    var p2 = new System.Windows.Point(1000, 0);
    var p3 = new System.Windows.Point(0, 0);
    var p4 = new System.Windows.Point(0, 1000);
    var v1 = UtilsVector.CreateVector(p1, p2);
    var v2 = UtilsVector.CreateVector(p3, p4);
    var p5 = p1 + v1 * 10 * scale;
    var p6 = p1 + v2 * 10 * scale;
    _ = new Angular2D(_canvas, scale, p1, p5, p6);

    // Filled rectangles
    var poly1 = new Polygon2D(
      _canvas,
      new System.Windows.Media.PointCollection(new[]
      {
        new System.Windows.Point(2000, 0),
        new System.Windows.Point(4000, 0),
        new System.Windows.Point(4000, 2000),
        new System.Windows.Point(2000, 2000)
      }),
      2,
      System.Windows.Media.Brushes.Black,
      System.Windows.Media.Brushes.LightBlue);
    _allShapes.Add(poly1);

    var poly2 = new Polygon2D(
      _canvas,
      new System.Windows.Media.PointCollection(new[]
      {
        new System.Windows.Point(4200, 0),
        new System.Windows.Point(6200, 0),
        new System.Windows.Point(6200, 2000),
        new System.Windows.Point(4200, 2000)
      }),
      2,
      System.Windows.Media.Brushes.Black,
      System.Windows.Media.Brushes.LightGreen);
    _allShapes.Add(poly2);

    var poly3 = new Polygon2D(
      _canvas,
      new System.Windows.Media.PointCollection(new[]
      {
        new System.Windows.Point(2000, 2200),
        new System.Windows.Point(4000, 2200),
        new System.Windows.Point(4000, 4200),
        new System.Windows.Point(2000, 4200)
      }),
      2,
      System.Windows.Media.Brushes.Black,
      System.Windows.Media.Brushes.LightYellow);
    _allShapes.Add(poly3);

    // Text annotation
    _ = new TextBlock2D(_canvas, "Canvas Test", 10, new System.Windows.Point(2000, -200), System.Windows.Media.Brushes.Black);

    // Grid annotations
    var grid1 = new Grid2D(_canvas, scale, "A1", new System.Windows.Point(0, 0), new System.Windows.Point(8000, 0), EnumGridSymbolStyle.Circle, EnumGridSymbolStyle.Circle);
    var grid2 = new Grid2D(_canvas, scale, "B1", new System.Windows.Point(8000, 0), new System.Windows.Point(8000, 5000), EnumGridSymbolStyle.Circle, EnumGridSymbolStyle.Circle);
    _allAnnotations.Add(grid1);
    _allAnnotations.Add(grid2);
  }
}

public class ScaleModel
{
  public int Scale { get; }
  public string ScaleText { get; }

  public ScaleModel(int scale)
  {
    Scale = scale;
    ScaleText = $"1 : {scale}";
  }
}
