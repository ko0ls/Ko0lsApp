using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Canvas.Shapes;

namespace AutoCADTools.Presentation.Drawing;

public class SwallowFoundationPreviewDrawer
{
  private readonly global::System.Windows.Controls.Canvas _canvas;

  private const double LineThickness = 1.0;
  private static readonly Brush BlackBrush = Brushes.Black;
  private static readonly Brush GrayBrush = Brushes.Gray;
  private static readonly Brush DarkGrayBrush = Brushes.DarkGray;
  private static readonly Brush WhiteBrush = Brushes.White;
  private static readonly Brush LightGrayBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
  private static readonly Brush RedBrush = Brushes.Red;
  private static readonly DoubleCollection DashPattern = new DoubleCollection([4, 2]);

  public SwallowFoundationPreviewDrawer(global::System.Windows.Controls.Canvas canvas)
  {
    _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
  }

  public void RefreshDrawing(SwallowFoundationModel model)
  {
    _canvas.Children.Clear();
    if (model == null) return;

    double canvasWidth = _canvas.ActualWidth;
    double canvasHeight = _canvas.ActualHeight;
    if (canvasWidth < 1 || canvasHeight < 1) return;

    double planHeight = canvasHeight * 0.48;
    double margin = 20.0;

    double planScale = CalculatePlanScale(model, canvasWidth, planHeight, margin);
    double sectionScale = CalculateSectionScale(model, canvasWidth, planHeight, margin);

    DrawPlan(model, margin, margin, planScale);
    DrawSection(model, margin, margin + planHeight, sectionScale);
  }

  private double CalculatePlanScale(SwallowFoundationModel model, double canvasWidth, double canvasHeight, double margin)
  {
    double totalWidthPx = model.LengthX * model.Scale + 2 * model.ConcretePadWidth;
    double totalHeightPx = model.LengthY * model.Scale + 2 * model.ConcretePadWidth;
    double availW = canvasWidth - 2 * margin;
    double availH = canvasHeight - 2 * margin;
    if (availW < 1 || availH < 1) return 1;
    return Math.Min(availW / totalWidthPx, availH / totalHeightPx) * 0.9;
  }

  private double CalculateSectionScale(SwallowFoundationModel model, double canvasWidth, double canvasHeight, double margin)
  {
    double totalWidthPx = model.LengthX * model.Scale + 2 * model.ConcretePadWidth;
    double totalHeightPx = model.ColumnWidthX + model.StepHeightH1 + model.StepHeightH2 + model.ConcretePadThickness;
    if (totalHeightPx < 1) totalHeightPx = 1;
    double availW = canvasWidth - 2 * margin;
    double availH = canvasHeight - 2 * margin;
    if (availW < 1 || availH < 1) return 1;
    return Math.Min(availW / totalWidthPx, availH / totalHeightPx) * 0.9;
  }

  public void DrawPlan(SwallowFoundationModel model, double originX, double originY, double scalePreview)
  {
    double s = scalePreview;

    double Lx = model.LengthX * s;
    double Ly = model.LengthY * s;
    double padW = model.ConcretePadWidth * s;
    double padH = model.ConcretePadWidth * s;
    double colPosX = model.ColumnPositionX * s;
    double colPosY = model.ColumnPositionY * s;
    double colW = model.ColumnWidthX * s;
    double colH = model.ColumnWidthY * s;

    double totalW = Lx + 2 * padW;
    double totalH = Ly + 2 * padH;

    double footingLeft = originX + padW;
    double footingTop = originY + padH;

    // --- Concrete Pad (bleed area) ---
    var padRect = new Rectangle
    {
      Width = totalW,
      Height = totalH,
      Stroke = DarkGrayBrush,
      StrokeThickness = LineThickness,
      StrokeDashArray = DashPattern,
      Fill = LightGrayBrush
    };
    global::System.Windows.Controls.Canvas.SetLeft(padRect, originX);
    global::System.Windows.Controls.Canvas.SetTop(padRect, originY);
    global::System.Windows.Controls.Canvas.SetZIndex(padRect, 0);
    _canvas.Children.Add(padRect);

    // --- Footing outline ---
    var footingRect = new Rectangle
    {
      Width = Lx,
      Height = Ly,
      Stroke = BlackBrush,
      StrokeThickness = LineThickness,
      Fill = Brushes.Transparent
    };
    global::System.Windows.Controls.Canvas.SetLeft(footingRect, footingLeft);
    global::System.Windows.Controls.Canvas.SetTop(footingRect, footingTop);
    global::System.Windows.Controls.Canvas.SetZIndex(footingRect, 1);
    _canvas.Children.Add(footingRect);

    // --- Axis lines (dashed gray) ---
    double axCx = originX + totalW / 2;
    double axCy = originY + totalH / 2;

    var vAxis = new Line
    {
      X1 = axCx,
      Y1 = originY,
      X2 = axCx,
      Y2 = originY + totalH,
      Stroke = GrayBrush,
      StrokeThickness = LineThickness,
      StrokeDashArray = DashPattern
    };
    global::System.Windows.Controls.Canvas.SetZIndex(vAxis, 2);
    _canvas.Children.Add(vAxis);

    var hAxis = new Line
    {
      X1 = originX,
      Y1 = axCy,
      X2 = originX + totalW,
      Y2 = axCy,
      Stroke = GrayBrush,
      StrokeThickness = LineThickness,
      StrokeDashArray = DashPattern
    };
    global::System.Windows.Controls.Canvas.SetZIndex(hAxis, 2);
    _canvas.Children.Add(hAxis);

    // --- Column ---
    double colLeft = footingLeft + colPosX;
    double colTop = footingTop + colPosY;

    if (model.IsRectangularColumn)
    {
      var colRect = new Rectangle
      {
        Width = colW,
        Height = colH,
        Stroke = BlackBrush,
        StrokeThickness = LineThickness,
        Fill = WhiteBrush
      };
      global::System.Windows.Controls.Canvas.SetLeft(colRect, colLeft);
      global::System.Windows.Controls.Canvas.SetTop(colRect, colTop);
      global::System.Windows.Controls.Canvas.SetZIndex(colRect, 3);
      _canvas.Children.Add(colRect);
    }
    else if (model.IsCircularColumn)
    {
      var colEllipse = new Ellipse
      {
        Width = colW,
        Height = colH,
        Stroke = BlackBrush,
        StrokeThickness = LineThickness,
        Fill = WhiteBrush
      };
      global::System.Windows.Controls.Canvas.SetLeft(colEllipse, colLeft);
      global::System.Windows.Controls.Canvas.SetTop(colEllipse, colTop);
      global::System.Windows.Controls.Canvas.SetZIndex(colEllipse, 3);
      _canvas.Children.Add(colEllipse);
    }

    // --- Dimension labels ---
    double dimFontSize = Math.Max(9, 11 * scalePreview / model.Scale);

    AddTextBlock(
      "Label.LengthX".GetString() + "=" + model.LengthX.ToString(CultureInfo.InvariantCulture),
      footingLeft + Lx / 2,
      footingTop + Ly + padH * 0.3,
      dimFontSize, BlackBrush,
      CanvasSetPosition.CenterMiddle, 4);

    AddTextBlock(
      "Label.LengthY".GetString() + "=" + model.LengthY.ToString(CultureInfo.InvariantCulture),
      footingLeft + Lx + padW * 0.3,
      footingTop + Ly / 2,
      dimFontSize, BlackBrush,
      CanvasSetPosition.TopLeft, 4);

    // --- Plan title ---
    AddTextBlock(
      "SwallowFoundation.View.Plan".GetString(),
      originX,
      originY - dimFontSize - 6,
      dimFontSize + 2, BlackBrush,
      CanvasSetPosition.BottomLeft, 4, fontWeight: FontWeights.Bold);
  }

  public void DrawSection(SwallowFoundationModel model, double originX, double originY, double scalePreview)
  {
    double s = scalePreview;

    double Lx = model.LengthX * s;
    double padW = model.ConcretePadWidth * s;
    double padThick = model.ConcretePadThickness * s;
    double H1 = model.StepHeightH1 * s;
    double H2 = model.StepHeightH2 * s;
    double colW = model.ColumnWidthX * s;

    double totalW = Lx + 2 * padW;
    double totalH = H1 + H2 + padThick + Math.Max(colW, H1); // total vertical extent
    if (totalH < 1) totalH = 1;

    double footingLeft = originX + padW;
    double padTop = originY + totalH - padThick;

    // --- Concrete Pad (bottom, widest) ---
    var padRect = new Rectangle
    {
      Width = totalW,
      Height = padThick,
      Stroke = DarkGrayBrush,
      StrokeThickness = LineThickness,
      StrokeDashArray = DashPattern,
      Fill = LightGrayBrush
    };
    global::System.Windows.Controls.Canvas.SetLeft(padRect, originX);
    global::System.Windows.Controls.Canvas.SetTop(padRect, padTop);
    global::System.Windows.Controls.Canvas.SetZIndex(padRect, 0);
    _canvas.Children.Add(padRect);

    // --- Step H2 (footing lower step) ---
    double step2Top = padTop - H2;
    if (H2 > 0)
    {
      var step2Rect = new Rectangle
      {
        Width = Lx,
        Height = H2,
        Stroke = BlackBrush,
        StrokeThickness = LineThickness,
        Fill = Brushes.Transparent
      };
      global::System.Windows.Controls.Canvas.SetLeft(step2Rect, footingLeft);
      global::System.Windows.Controls.Canvas.SetTop(step2Rect, step2Top);
      global::System.Windows.Controls.Canvas.SetZIndex(step2Rect, 1);
      _canvas.Children.Add(step2Rect);
    }

    // --- Step H1 (upper step / vat area) ---
    double step1Top = step2Top - H1;
    double step1H = H1;

    double colStubLeft = originX + totalW / 2 - colW / 2;
    double colStubTop = originY;

    if (Math.Abs(H1 - H2) < 1e-6 || H1 < 1e-6)
    {
      // Rectangular step H1 (no vat)
      if (step1H > 0)
      {
        var step1Rect = new Rectangle
        {
          Width = Lx,
          Height = step1H,
          Stroke = BlackBrush,
          StrokeThickness = LineThickness,
          Fill = Brushes.Transparent
        };
        global::System.Windows.Controls.Canvas.SetLeft(step1Rect, footingLeft);
        global::System.Windows.Controls.Canvas.SetTop(step1Rect, step1Top);
        global::System.Windows.Controls.Canvas.SetZIndex(step1Rect, 1);
        _canvas.Children.Add(step1Rect);
      }
    }
    else
    {
      // Trapezoid vat: narrower at top (column stub width)
      var vatPoints = new PointCollection
      {
        new Point(footingLeft, step1Top + step1H),       // bottom-left
        new Point(footingLeft + Lx, step1Top + step1H), // bottom-right
        new Point(colStubLeft + colW, step1Top),          // top-right (at column stub edge)
        new Point(colStubLeft, step1Top)                  // top-left
      };
      var vatPolygon = new Polygon
      {
        Points = vatPoints,
        Stroke = BlackBrush,
        StrokeThickness = LineThickness,
        Fill = Brushes.Transparent
      };
      global::System.Windows.Controls.Canvas.SetZIndex(vatPolygon, 1);
      _canvas.Children.Add(vatPolygon);
    }

    // --- Column stub (rectangular, top center) ---
    double colStubHeight = totalH - padThick - H2 - H1;
    if (colStubHeight < 0) colStubHeight = 0;

    if (colStubHeight > 0)
    {
      var colRect = new Rectangle
      {
        Width = colW,
        Height = colStubHeight,
        Stroke = BlackBrush,
        StrokeThickness = LineThickness,
        Fill = WhiteBrush
      };
      global::System.Windows.Controls.Canvas.SetLeft(colRect, colStubLeft);
      global::System.Windows.Controls.Canvas.SetTop(colRect, colStubTop);
      global::System.Windows.Controls.Canvas.SetZIndex(colRect, 2);
      _canvas.Children.Add(colRect);
    }

    // --- Column Rebars ---
    if (model.DrawColumnRebar)
    {
      double rebarDia = ParseRebarDiameter(model.ColumnRebar) * s;
      if (rebarDia > 0)
      {
        DrawSectionColumnRebars(model, colStubLeft, colStubTop, colW, colStubHeight, rebarDia);
      }
    }

    // --- Elevation labels ---
    double dimFontSize = Math.Max(9, 11 * scalePreview / model.Scale);

    AddTextBlock(
      "SwallowFoundation.Label.Elevation".GetString() + " " + model.FloorLevel.ToString("F0", CultureInfo.InvariantCulture),
      originX,
      originY - dimFontSize - 4,
      dimFontSize, GrayBrush,
      CanvasSetPosition.BottomLeft, 4);

    AddTextBlock(
      "SwallowFoundation.Label.Elevation".GetString() + " " + model.FoundationBottomLevel.ToString("F0", CultureInfo.InvariantCulture),
      originX,
      originY + totalH + 4,
      dimFontSize, GrayBrush,
      CanvasSetPosition.TopLeft, 4);

    // --- Vertical dimension labels H1, H2 ---
    double labelX = originX + totalW + padW * 0.15;

    if (H2 > 0)
    {
      AddTextBlock(
        "SwallowFoundation.Label.StepH2".GetString() + "=" + model.StepHeightH2.ToString("F0", CultureInfo.InvariantCulture),
        labelX,
        padTop - H2 / 2,
        dimFontSize, BlackBrush,
        CanvasSetPosition.CenterMiddle, 4);
    }

    if (H1 > 0)
    {
      AddTextBlock(
        "SwallowFoundation.Label.StepH1".GetString() + "=" + model.StepHeightH1.ToString("F0", CultureInfo.InvariantCulture),
        labelX,
        step2Top - H1 / 2,
        dimFontSize, BlackBrush,
        CanvasSetPosition.CenterMiddle, 4);
    }

    // --- Section title ---
    AddTextBlock(
      "SwallowFoundation.View.Section".GetString(),
      originX,
      originY - dimFontSize - 6,
      dimFontSize + 2, BlackBrush,
      CanvasSetPosition.BottomLeft, 4, fontWeight: FontWeights.Bold);
  }

  private void DrawSectionColumnRebars(
    SwallowFoundationModel model,
    double colLeft,
    double colTop,
    double colWidth,
    double colHeight,
    double rebarDia)
  {
    int countX = Math.Max(2, model.ColumnRebarCountX);
    int countY = Math.Max(2, model.ColumnRebarCountY);
    double cover = model.Cover * model.Scale;

    double innerW = colWidth - 2 * cover;
    double innerH = colHeight - 2 * cover;
    if (innerW < rebarDia || innerH < rebarDia || cover < 0) return;

    if (model.IsRectangularColumn)
    {
      // Along left/right edges: countY rebars each
      double stepY = innerH / Math.Max(countY - 1, 1);
      for (int i = 0; i < countY; i++)
      {
        double cy = colTop + cover + i * stepY;
        AddRebarCircle(colLeft + cover + rebarDia / 2, cy, rebarDia);
        AddRebarCircle(colLeft + colWidth - cover - rebarDia / 2, cy, rebarDia);
      }

      // Along top/bottom edges: countX rebars each, excluding corners (handled by sides)
      double stepX = innerW / Math.Max(countX - 1, 1);
      for (int i = 0; i < countX; i++)
      {
        double cx = colLeft + cover + i * stepX;
        AddRebarCircle(cx, colTop + cover + rebarDia / 2, rebarDia);
        AddRebarCircle(cx, colTop + colHeight - cover - rebarDia / 2, rebarDia);
      }
    }
    else if (model.IsCircularColumn)
    {
      // Arrange rebars evenly around perimeter
      int totalCount = Math.Max(countX, countY) * 4;
      double r = (Math.Min(colWidth, colHeight) / 2) - cover - rebarDia / 2;
      double cx = colLeft + colWidth / 2;
      double cy = colTop + colHeight / 2;
      if (r < rebarDia / 2) return;

      for (int i = 0; i < totalCount; i++)
      {
        double angle = 2 * Math.PI * i / totalCount;
        double rx = cx + r * Math.Cos(angle);
        double ry = cy + r * Math.Sin(angle);
        AddRebarCircle(rx, ry, rebarDia);
      }
    }
  }

  private void AddRebarCircle(double cx, double cy, double dia)
  {
    var circle = new Ellipse
    {
      Width = dia,
      Height = dia,
      Stroke = RedBrush,
      StrokeThickness = LineThickness,
      Fill = RedBrush
    };
    global::System.Windows.Controls.Canvas.SetLeft(circle, cx - dia / 2);
    global::System.Windows.Controls.Canvas.SetTop(circle, cy - dia / 2);
    global::System.Windows.Controls.Canvas.SetZIndex(circle, 3);
    _canvas.Children.Add(circle);
  }

  private enum CanvasSetPosition
  {
    CenterMiddle,
    TopLeft,
    BottomLeft
  }

  private void AddTextBlock(
    string text,
    double x,
    double y,
    double fontSize,
    Brush foreground,
    CanvasSetPosition position,
    int zIndex = 0,
    FontWeight fontWeight = default)
  {
    var tb = new System.Windows.Controls.TextBlock
    {
      Text = text,
      FontSize = fontSize,
      Foreground = foreground,
      FontFamily = new FontFamily("Arial"),
      FontWeight = fontWeight == default ? FontWeights.Normal : fontWeight
    };

    tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    var sz = tb.DesiredSize;
    if (sz.Width < 1) sz.Width = 1;
    if (sz.Height < 1) sz.Height = 1;

    double left = position switch
    {
      CanvasSetPosition.CenterMiddle => x - sz.Width / 2,
      CanvasSetPosition.TopLeft => x,
      CanvasSetPosition.BottomLeft => x,
      _ => x
    };

    double top = position switch
    {
      CanvasSetPosition.CenterMiddle => y - sz.Height / 2,
      CanvasSetPosition.TopLeft => y,
      CanvasSetPosition.BottomLeft => y - sz.Height,
      _ => y
    };

    global::System.Windows.Controls.Canvas.SetLeft(tb, left);
    global::System.Windows.Controls.Canvas.SetTop(tb, top);
    global::System.Windows.Controls.Canvas.SetZIndex(tb, zIndex);
    _canvas.Children.Add(tb);
  }

  private static double ParseRebarDiameter(string rebarSpec)
  {
    if (string.IsNullOrWhiteSpace(rebarSpec)) return 0;
    int dIdx = rebarSpec.LastIndexOf('d');
    if (dIdx < 0) dIdx = rebarSpec.LastIndexOf('D');
    if (dIdx >= 0 && dIdx < rebarSpec.Length - 1)
    {
      var num = rebarSpec.Substring(dIdx + 1).Trim();
      if (double.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out var dia))
        return dia;
    }
    if (double.TryParse(rebarSpec, NumberStyles.Any, CultureInfo.InvariantCulture, out var direct))
      return direct;
    return 0;
  }
}
