#if NET8_0_OR_GREATER
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class Line2DTests : IDisposable
{
  private Canvas _canvas = null!;

  public void Dispose()
  {
    _canvas = null!;
    GC.SuppressFinalize(this);
  }

  #region Constructor Validation

  [Fact]
  public void Constructor_WithNullCanvas_DoesNotThrow()
  {
    StaThread.Run(() =>
    {
      var act = () => new Line2D(null, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Black);

      act.Should().NotThrow();
    });
  }

  [Fact]
  public void Constructor_WithInvalidStartPoint_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(double.NaN, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithInvalidEndPoint_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(0, 0), new Point(double.NaN, 10), EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithSameStartAndEndPoint_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(5, 5), new Point(5, 5), EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithZeroThickness_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 0, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithNullColor_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, null);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  #endregion

  #region Constructor Valid Cases

  [Fact]
  public void Constructor_WithValidParams_AddsLineToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);

      _canvas.Children.Count.Should().Be(1);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsCorrectCoordinates()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(1, 2), new Point(11, 12), EnumLineType.Solid, 2, Brushes.Blue);

      var line = line2D.GetLine();
      line.X1.Should().Be(1);
      line.Y1.Should().Be(2);
      line.X2.Should().Be(11);
      line.Y2.Should().Be(12);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 3, Brushes.Blue);

      var line = line2D.GetLine();
      line.StrokeThickness.Should().Be(3);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeColor()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Green);

      var line = line2D.GetLine();
      line.Stroke.Should().Be(Brushes.Green);
    });
  }

  [Fact]
  public void Constructor_WithAngle_SetsRotateTransform()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 0), EnumLineType.Solid, 2, Brushes.Blue, angle: 45);

      var line = line2D.GetLine();
      line.RenderTransform.Should().BeOfType<RotateTransform>();
    });
  }

  [Fact]
  public void Constructor_WithZeroAngle_DoesNotSetRotateTransform()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue, angle: 0);

      var line = line2D.GetLine();
      // WPF defaults RenderTransform to Transform.Identity, not null
      line.RenderTransform.Should().Be(Transform.Identity);
    });
  }

  #endregion

  #region StrokeDashArray

  [Fact]
  public void Constructor_WithSolidLineType_SetsEmptyStrokeDashArray()
  {
    int count = -1;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);
      count = line2D.GetLine().StrokeDashArray.Count;
    });
    count.Should().Be(0);
  }

  [Fact]
  public void Constructor_WithDashLineType_SetsCorrectStrokeDashArray()
  {
    int count = -1;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Dash, 2, Brushes.Blue);
      count = line2D.GetLine().StrokeDashArray.Count;
    });
    count.Should().BeGreaterThan(0);
  }

  [Fact]
  public void Constructor_WithDashDotLineType_SetsCorrectStrokeDashArray()
  {
    int count = -1;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.DashDot, 2, Brushes.Blue);
      count = line2D.GetLine().StrokeDashArray.Count;
    });
    count.Should().BeGreaterThan(0);
  }

  #endregion

  #region MakeHighLight

  [Fact]
  public void MakeHighLight_SetsRedStroke()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);

      line2D.MakeHighLight();

      var line = line2D.GetLine();
      line.Stroke.Should().Be(Brushes.Red);
    });
  }

  [Fact]
  public void MakeHighLight_IncreasesStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 2.0;
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, originalThickness, Brushes.Blue);

      line2D.MakeHighLight();

      var line = line2D.GetLine();
      line.StrokeThickness.Should().Be(5 * originalThickness);
    });
  }

  [Fact]
  public void MakeHighLight_SetsZIndexToOne()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);

      line2D.MakeHighLight();

      Canvas.GetZIndex(line2D.GetLine()).Should().Be(1);
    });
  }

  #endregion

  #region ResetHighLight

  [Fact]
  public void ResetHighLight_RestoresOriginalStroke()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);

      line2D.MakeHighLight();
      line2D.ResetHighLight();

      var line = line2D.GetLine();
      line.Stroke.Should().Be(Brushes.Blue);
    });
  }

  [Fact]
  public void ResetHighLight_RestoresOriginalThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 3.0;
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, originalThickness, Brushes.Blue);

      line2D.MakeHighLight();
      line2D.ResetHighLight();

      var line = line2D.GetLine();
      line.StrokeThickness.Should().Be(originalThickness);
    });
  }

  [Fact]
  public void ResetHighLight_ResetsZIndexToZero()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 10), EnumLineType.Solid, 2, Brushes.Blue);

      line2D.MakeHighLight();
      line2D.ResetHighLight();

      Canvas.GetZIndex(line2D.GetLine()).Should().Be(0);
    });
  }

  #endregion

  #region CheckMoveOver

  [Fact]
  public void CheckMoveOver_WithPointOnLine_ReturnsTrue()
  {
    bool result = false;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 0), EnumLineType.Solid, 2, Brushes.Blue);
      result = line2D.CheckMoveOver(new Point(5, 0));
    });
    result.Should().BeTrue();
  }

  [Fact]
  public void CheckMoveOver_WithPointFarFromLine_ReturnsFalse()
  {
    bool result = true;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var line2D = new Line2D(_canvas, new Point(0, 0), new Point(10, 0), EnumLineType.Solid, 2, Brushes.Blue);
      result = line2D.CheckMoveOver(new Point(5, 100));
    });
    result.Should().BeFalse();
  }

  #endregion
}

#endif
