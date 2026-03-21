#if NET8_0_OR_GREATER
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class Polyline2DTests : IDisposable
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var act = () => new Polyline2D(null, points, EnumLineType.Solid, 2, Brushes.Black);

      act.Should().NotThrow();
    });
  }

  [Fact]
  public void Constructor_WithNullPoints_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Polyline2D(_canvas, default!, EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithEmptyPoints_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Polyline2D(_canvas, new PointCollection(), EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithSinglePoint_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(5, 5) };

      new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithZeroThickness_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };

      new Polyline2D(_canvas, points, EnumLineType.Solid, 0, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithNullColor_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };

      new Polyline2D(_canvas, points, EnumLineType.Solid, 2, null);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  #endregion

  #region Constructor Valid Cases

  [Fact]
  public void Constructor_WithValidParams_AddsPolylineToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10), new Point(20, 0) };

      new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      _canvas.Children.Count.Should().Be(1);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsCorrectPoints()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(1, 2), new Point(11, 12), new Point(21, 2) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      var polyline = polyline2D.GetPolyline();
      polyline.Points.Should().HaveCount(3);
      polyline.Points[0].Should().Be(new Point(1, 2));
      polyline.Points[1].Should().Be(new Point(11, 12));
      polyline.Points[2].Should().Be(new Point(21, 2));
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 3, Brushes.Blue);

      var polyline = polyline2D.GetPolyline();
      polyline.StrokeThickness.Should().Be(3);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeColor()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Green);

      var polyline = polyline2D.GetPolyline();
      polyline.Stroke.Should().Be(Brushes.Green);
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);
      count = polyline2D.GetPolyline().StrokeDashArray.Count;
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Dash, 2, Brushes.Blue);
      count = polyline2D.GetPolyline().StrokeDashArray.Count;
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.DashDot, 2, Brushes.Blue);
      count = polyline2D.GetPolyline().StrokeDashArray.Count;
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      polyline2D.MakeHighLight();

      var polyline = polyline2D.GetPolyline();
      polyline.Stroke.Should().Be(Brushes.Red);
    });
  }

  [Fact]
  public void MakeHighLight_IncreasesStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 2.0;
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, originalThickness, Brushes.Blue);

      polyline2D.MakeHighLight();

      var polyline = polyline2D.GetPolyline();
      polyline.StrokeThickness.Should().Be(5 * originalThickness);
    });
  }

  [Fact]
  public void MakeHighLight_SetsZIndexToOne()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      polyline2D.MakeHighLight();

      Canvas.GetZIndex(polyline2D.GetPolyline()).Should().Be(1);
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
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      polyline2D.MakeHighLight();
      polyline2D.ResetHighLight();

      var polyline = polyline2D.GetPolyline();
      polyline.Stroke.Should().Be(Brushes.Blue);
    });
  }

  [Fact]
  public void ResetHighLight_RestoresOriginalThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 3.0;
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, originalThickness, Brushes.Blue);

      polyline2D.MakeHighLight();
      polyline2D.ResetHighLight();

      var polyline = polyline2D.GetPolyline();
      polyline.StrokeThickness.Should().Be(originalThickness);
    });
  }

  [Fact]
  public void ResetHighLight_ResetsZIndexToZero()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      polyline2D.MakeHighLight();
      polyline2D.ResetHighLight();

      Canvas.GetZIndex(polyline2D.GetPolyline()).Should().Be(0);
    });
  }

  #endregion

  #region CheckMoveOver

  [Fact]
  public void CheckMoveOver_WithPointOnPolyline_ReturnsTrue()
  {
    bool result = false;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);
      result = polyline2D.CheckMoveOver(new Point(5, 0));
    });
    result.Should().BeTrue();
  }

  [Fact]
  public void CheckMoveOver_WithPointFarFromPolyline_ReturnsFalse()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var points = new PointCollection { new Point(0, 0), new Point(10, 0), new Point(10, 10) };
      var polyline2D = new Polyline2D(_canvas, points, EnumLineType.Solid, 2, Brushes.Blue);

      var result = polyline2D.CheckMoveOver(new Point(100, 100));

      result.Should().BeFalse();
    });
  }

  #endregion
}

#endif
