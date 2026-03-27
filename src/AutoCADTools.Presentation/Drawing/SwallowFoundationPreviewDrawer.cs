using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;

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

  public void RefreshDrawing(SwallowFoundationModel? model)
  {
    _canvas.Children.Clear();
    if (model == null) return;

    var canvasWidth = _canvas.ActualWidth;
    var canvasHeight = _canvas.ActualHeight;
    if (canvasWidth < 1 || canvasHeight < 1) return;

    var planHeight = canvasHeight * 0.48;
    var margin = 20.0;

    var planScale = CalculatePlanScale(model, canvasWidth, planHeight, margin);
    var sectionScale = CalculateSectionScale(model, canvasWidth, planHeight, margin);

    if (double.IsNaN(planScale) || double.IsInfinity(planScale) ||
        double.IsNaN(sectionScale) || double.IsInfinity(sectionScale))
    {
      return;
    }

    DrawPlan(model, margin, margin, planScale);
    DrawSection(model, margin, margin + planHeight, sectionScale);
  }

  private double CalculatePlanScale(SwallowFoundationModel model, double canvasWidth, double canvasHeight, double margin)
  {
    var totalWidthPx = model.LengthX * model.Scale + 2 * model.ConcretePadExtension;
    var totalHeightPx = model.LengthY * model.Scale + 2 * model.ConcretePadExtension;
    var availW = canvasWidth - 2 * margin;
    var availH = canvasHeight - 2 * margin;
    if (availW < 1 || availH < 1) return 1;

    if (double.IsNaN(availW) || double.IsInfinity(availW) ||
        double.IsNaN(availH) || double.IsInfinity(availH) ||
        double.IsNaN(totalWidthPx) || double.IsInfinity(totalWidthPx) ||
        double.IsNaN(totalHeightPx) || double.IsInfinity(totalHeightPx) ||
        availW < 1 || availH < 1 ||
        totalWidthPx < 1e-9 || totalHeightPx < 1e-9)
    {
      return 1;
    }

    return Math.Min(availW / totalWidthPx, availH / totalHeightPx) * 0.9;
  }

  private double CalculateSectionScale(SwallowFoundationModel model, double canvasWidth, double canvasHeight, double margin)
  {
    var totalWidthPx = model.LengthX * model.Scale + 2 * model.ConcretePadExtension;
    var totalHeightPx = model.ColumnWidthX + model.StepHeightH1 + model.StepHeightH2 + model.ConcretePadThickness;
    var availW = canvasWidth - 2 * margin;
    var availH = canvasHeight - 2 * margin;
    if (availW < 1 || availH < 1) return 1;

    if (double.IsNaN(availW) || double.IsInfinity(availW) ||
        double.IsNaN(availH) || double.IsInfinity(availH) ||
        double.IsNaN(totalWidthPx) || double.IsInfinity(totalWidthPx) ||
        double.IsNaN(totalHeightPx) || double.IsInfinity(totalHeightPx) ||
        availW < 1 || availH < 1 ||
        totalWidthPx < 1e-9 || totalHeightPx < 1e-9)
    {
      return 1;
    }

    return Math.Min(availW / totalWidthPx, availH / totalHeightPx) * 0.9;
  }

  public void DrawPlan(SwallowFoundationModel model, double originX, double originY, double scalePreview)
  {
    if (double.IsNaN(scalePreview) || double.IsInfinity(scalePreview)) return;

    var s = scalePreview;

    var Lx = model.LengthX * s;
    var Ly = model.LengthY * s;
    var padW = model.ConcretePadExtension * s;
    var padH = model.ConcretePadExtension * s;
    var colPosX = model.ColumnPositionX * s;
    var colPosY = model.ColumnPositionY * s;
    var colW = model.ColumnWidthX * s;
    var colH = model.ColumnWidthY * s;

    var totalW = Lx + 2 * padW;
    var totalH = Ly + 2 * padH;

    var footingLeft = originX + padW;
    var footingTop = originY + padH;

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
    System.Windows.Controls.Canvas.SetLeft(padRect, originX);
    System.Windows.Controls.Canvas.SetTop(padRect, originY);
    System.Windows.Controls.Panel.SetZIndex(padRect, 0);
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
    System.Windows.Controls.Canvas.SetLeft(footingRect, footingLeft);
    System.Windows.Controls.Canvas.SetTop(footingRect, footingTop);
    System.Windows.Controls.Panel.SetZIndex(footingRect, 1);
    _canvas.Children.Add(footingRect);

    // --- Axis lines (dashed gray) ---
    var axCx = originX + totalW / 2;
    var axCy = originY + totalH / 2;

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
    System.Windows.Controls.Panel.SetZIndex(vAxis, 2);
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
    System.Windows.Controls.Panel.SetZIndex(hAxis, 2);
    _canvas.Children.Add(hAxis);

    // --- Column ---
    var colLeft = footingLeft + colPosX;
    var colTop = footingTop + colPosY;

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
      System.Windows.Controls.Canvas.SetLeft(colRect, colLeft);
      System.Windows.Controls.Canvas.SetTop(colRect, colTop);
      System.Windows.Controls.Panel.SetZIndex(colRect, 3);
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
      System.Windows.Controls.Canvas.SetLeft(colEllipse, colLeft);
      System.Windows.Controls.Canvas.SetTop(colEllipse, colTop);
      System.Windows.Controls.Panel.SetZIndex(colEllipse, 3);
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
    if (double.IsNaN(scalePreview) || double.IsInfinity(scalePreview)) return;

    var s = scalePreview;

    var Lx = model.LengthX * s;
    var padW = model.ConcretePadExtension * s;
    var padThick = model.ConcretePadThickness * s;
    var H1 = model.StepHeightH1 * s;
    var H2 = model.StepHeightH2 * s;
    var colW = model.ColumnWidthX * s;

    var totalW = Lx + 2 * padW;
    var totalH = H1 + H2 + padThick + Math.Max(colW, H1); // total vertical extent
    if (totalH < 1) totalH = 1;

    var footingLeft = originX + padW;
    var padTop = originY + totalH - padThick;

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
    System.Windows.Controls.Canvas.SetLeft(padRect, originX);
    System.Windows.Controls.Canvas.SetTop(padRect, padTop);
    System.Windows.Controls.Panel.SetZIndex(padRect, 0);
    _canvas.Children.Add(padRect);

    // --- Step H2 (footing lower step) ---
    var step2Top = padTop - H2;
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
      System.Windows.Controls.Canvas.SetLeft(step2Rect, footingLeft);
      System.Windows.Controls.Canvas.SetTop(step2Rect, step2Top);
      System.Windows.Controls.Panel.SetZIndex(step2Rect, 1);
      _canvas.Children.Add(step2Rect);
    }

    // --- Step H1 (upper step / vat area) ---
    var step1Top = step2Top - H1;
    var step1H = H1;

    var colStubLeft = originX + totalW / 2 - colW / 2;
    var colStubTop = originY;

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
        System.Windows.Controls.Canvas.SetLeft(step1Rect, footingLeft);
        System.Windows.Controls.Canvas.SetTop(step1Rect, step1Top);
        System.Windows.Controls.Panel.SetZIndex(step1Rect, 1);
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
      System.Windows.Controls.Panel.SetZIndex(vatPolygon, 1);
      _canvas.Children.Add(vatPolygon);
    }

    // --- Column stub (rectangular, top center) ---
    var colStubHeight = totalH - padThick - H2 - H1;
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
      System.Windows.Controls.Canvas.SetLeft(colRect, colStubLeft);
      System.Windows.Controls.Canvas.SetTop(colRect, colStubTop);
      System.Windows.Controls.Panel.SetZIndex(colRect, 2);
      _canvas.Children.Add(colRect);
    }

    // --- Column Rebars ---
    if (model.DrawColumnRebar)
    {
      var rebarDia = ParseRebarDiameter(model.ColumnRebar) * s;
      if (rebarDia > 0)
      {
        DrawSectionColumnRebars(model, colStubLeft, colStubTop, colW, colStubHeight, rebarDia);
      }
    }

    // --- Elevation labels ---
    var dimFontSize = Math.Max(9, 11 * scalePreview / model.Scale);

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
    var labelX = originX + totalW + padW * 0.15;

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
    var countX = Math.Max(2, model.ColumnRebarCountX);
    var countY = Math.Max(2, model.ColumnRebarCountY);
    var cover = model.Cover * model.Scale;

    var innerW = colWidth - 2 * cover;
    var innerH = colHeight - 2 * cover;
    if (innerW < rebarDia || innerH < rebarDia || cover < 0) return;

    double stepX = innerW / Math.Max(countX - 1, 1);
    if (model.IsRectangularColumn)
    {
      // Along left/right edges: countY rebars each
      var stepY = innerH / Math.Max(countY - 1, 1);
      for (var i = 0; i < countY; i++)
      {
        var cy = colTop + cover + i * stepY;
        AddRebarCircle(colLeft + cover + rebarDia / 2, cy, rebarDia);
        AddRebarCircle(colLeft + colWidth - cover - rebarDia / 2, cy, rebarDia);
      }

      // Along top/bottom edges: countX rebars each, excluding corners (handled by sides)
      for (var i = 0; i < countX; i++)
      {
        var cx = colLeft + cover + i * stepX;
        AddRebarCircle(cx, colTop + cover + rebarDia / 2, rebarDia);
        AddRebarCircle(cx, colTop + colHeight - cover - rebarDia / 2, rebarDia);
      }
    }
    else if (model.IsCircularColumn)
    {
      // Arrange rebars evenly around perimeter
      var totalCount = Math.Max(countX, countY) * 4;
      var r = (Math.Min(colWidth, colHeight) / 2) - cover - rebarDia / 2;
      var cx = colLeft + colWidth / 2;
      var cy = colTop + colHeight / 2;
      if (r < rebarDia / 2) return;

      for (var i = 0; i < totalCount; i++)
      {
        var angle = 2 * Math.PI * i / totalCount;
        var rx = cx + r * Math.Cos(angle);
        var ry = cy + r * Math.Sin(angle);
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
    System.Windows.Controls.Canvas.SetLeft(circle, cx - dia / 2);
    System.Windows.Controls.Canvas.SetTop(circle, cy - dia / 2);
    System.Windows.Controls.Panel.SetZIndex(circle, 3);
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

    var left = position switch
    {
      CanvasSetPosition.CenterMiddle => x - sz.Width / 2,
      _ => x
    };

    double top = position switch
    {
      CanvasSetPosition.CenterMiddle => y - sz.Height / 2,
      CanvasSetPosition.TopLeft => y,
      CanvasSetPosition.BottomLeft => y - sz.Height,
      _ => y
    };

    System.Windows.Controls.Canvas.SetLeft(tb, left);
    System.Windows.Controls.Canvas.SetTop(tb, top);
    System.Windows.Controls.Panel.SetZIndex(tb, zIndex);
    _canvas.Children.Add(tb);
  }

  private static double ParseRebarDiameter(string rebarSpec)
  {
    if (string.IsNullOrWhiteSpace(rebarSpec)) return 0;
    var dIdx = rebarSpec.LastIndexOf('d');
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
