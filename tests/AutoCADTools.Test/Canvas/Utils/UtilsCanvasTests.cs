#if NET8_0_OR_GREATER
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class UtilsCanvasTests
{
  #region GetMinMaxPoints

  [Fact]
  public void GetMinMaxPoints_ForLine_ReturnsCorrectBounds()
  {
    StaThread.Run(() =>
    {
      var line = new Line
      {
        X1 = 10,
        Y1 = 20,
        X2 = 50,
        Y2 = 80
      };

      var (minPoint, maxPoint) = line.GetMinMaxPoints();

      minPoint.X.Should().Be(10);
      minPoint.Y.Should().Be(20);
      maxPoint.X.Should().Be(50);
      maxPoint.Y.Should().Be(80);
    });
  }

  [Fact]
  public void GetMinMaxPoints_ForEllipse_ReturnsCorrectBounds()
  {
    StaThread.Run(() =>
    {
      var canvas = new Canvas();
      var ellipse = new Ellipse
      {
        Width = 40,
        Height = 30
      };
      Canvas.SetLeft(ellipse, 10);
      Canvas.SetTop(ellipse, 20);
      canvas.Children.Add(ellipse);

      var (minPoint, maxPoint) = ellipse.GetMinMaxPoints();

      minPoint.X.Should().BeApproximately(10, 0.1);
      minPoint.Y.Should().BeApproximately(20, 0.1);
      maxPoint.X.Should().BeApproximately(50, 0.1);
      maxPoint.Y.Should().BeApproximately(50, 0.1);
    });
  }

  #endregion

  #region GetMaxPoint

  [Fact]
  public void GetMaxPoint_ForLine_ReturnsCorrectMaxPoint()
  {
    StaThread.Run(() =>
    {
      var line = new Line
      {
        X1 = 10,
        Y1 = 5,
        X2 = 30,
        Y2 = 15
      };

      var max = line.GetMaxPoint();

      max.X.Should().Be(30);
      max.Y.Should().Be(15);
    });
  }

  [Fact]
  public void GetMaxPoint_ForEllipse_ReturnsRightBottomCorner()
  {
    StaThread.Run(() =>
    {
      var ellipse = new Ellipse
      {
        Width = 40,
        Height = 30
      };
      Canvas.SetLeft(ellipse, 10);
      Canvas.SetTop(ellipse, 20);

      var max = ellipse.GetMaxPoint();

      max.X.Should().Be(50);
      max.Y.Should().Be(50);
    });
  }

  [Fact]
  public void GetMaxPoint_ForPolygon_ReturnsMaxCoords()
  {
    StaThread.Run(() =>
    {
      var polygon = new Polygon
      {
        Points = new PointCollection {
          new Point(5, 10),
          new Point(20, 30),
          new Point(15, 5)
        }
      };

      var max = polygon.GetMaxPoint();

      max.X.Should().Be(20);
      max.Y.Should().Be(30);
    });
  }

  [Fact]
  public void GetMaxPoint_ForPolyline_ReturnsMaxCoords()
  {
    StaThread.Run(() =>
    {
      var polyline = new Polyline
      {
        Points = new PointCollection {
          new Point(3, 7),
          new Point(25, 40),
          new Point(10, 2)
        }
      };

      var max = polyline.GetMaxPoint();

      max.X.Should().Be(25);
      max.Y.Should().Be(40);
    });
  }

  [Fact]
  public void GetMaxPoint_ForUnknownElement_ReturnsEmptyPoint()
  {
    StaThread.Run(() =>
    {
      var border = new Border();

      var max = border.GetMaxPoint();

      max.Should().Be(new Point());
    });
  }

  #endregion

  #region GetMinPoint

  [Fact]
  public void GetMinPoint_ForLine_ReturnsCorrectMinPoint()
  {
    StaThread.Run(() =>
    {
      var line = new Line
      {
        X1 = 10,
        Y1 = 5,
        X2 = 30,
        Y2 = 15
      };

      var min = line.GetMinPoint();

      min.X.Should().Be(10);
      min.Y.Should().Be(5);
    });
  }

  [Fact]
  public void GetMinPoint_ForEllipse_ReturnsLeftTopCorner()
  {
    StaThread.Run(() =>
    {
      var ellipse = new Ellipse
      {
        Width = 40,
        Height = 30
      };
      Canvas.SetLeft(ellipse, 10);
      Canvas.SetTop(ellipse, 20);

      var min = ellipse.GetMinPoint();

      min.X.Should().Be(10);
      min.Y.Should().Be(20);
    });
  }

  [Fact]
  public void GetMinPoint_ForPolygon_ReturnsMinCoords()
  {
    StaThread.Run(() =>
    {
      var polygon = new Polygon
      {
        Points = new PointCollection {
          new Point(5, 10),
          new Point(20, 30),
          new Point(15, 5)
        }
      };

      var min = polygon.GetMinPoint();

      min.X.Should().Be(5);
      min.Y.Should().Be(5);
    });
  }

  [Fact]
  public void GetMinPoint_ForPolyline_ReturnsMinCoords()
  {
    StaThread.Run(() =>
    {
      var polyline = new Polyline
      {
        Points = new PointCollection {
          new Point(3, 7),
          new Point(25, 40),
          new Point(10, 2)
        }
      };

      var min = polyline.GetMinPoint();

      min.X.Should().Be(3);
      min.Y.Should().Be(2);
    });
  }

  [Fact]
  public void GetMinPoint_ForUnknownElement_ReturnsEmptyPoint()
  {
    StaThread.Run(() =>
    {
      var border = new Border();

      var min = border.GetMinPoint();

      min.Should().Be(new Point());
    });
  }

  #endregion

  #region CalculateCenter

  [Fact]
  public void CalculateCenter_WithValidPoints_ReturnsAveragePoint()
  {
    StaThread.Run(() =>
    {
      var points = new PointCollection {
        new Point(0, 0),
        new Point(10, 0),
        new Point(10, 10),
        new Point(0, 10)
      };

      var center = UtilsCanvas.CalculateCenter(points);

      center.X.Should().Be(5);
      center.Y.Should().Be(5);
    });
  }

  [Fact]
  public void CalculateCenter_WithSinglePoint_ReturnsSamePoint()
  {
    StaThread.Run(() =>
    {
      var points = new PointCollection { new Point(7, 3) };

      var center = UtilsCanvas.CalculateCenter(points);

      center.X.Should().Be(7);
      center.Y.Should().Be(3);
    });
  }

  [Fact]
  public void CalculateCenter_WithNullPoints_ThrowsArgumentException()
  {
    StaThread.Run(() =>
    {
      PointCollection? points = null;

      var act = () => UtilsCanvas.CalculateCenter(points!);

      act.Should().Throw<ArgumentException>();
    });
  }

  [Fact]
  public void CalculateCenter_WithEmptyPoints_ThrowsArgumentException()
  {
    StaThread.Run(() =>
    {
      var points = new PointCollection();

      var act = () => UtilsCanvas.CalculateCenter(points);

      act.Should().Throw<ArgumentException>();
    });
  }

  #endregion

  #region GetLineThickness

  [Fact]
  public void GetLineThickness_ReturnsScaledThickness()
  {
    StaThread.Run(() =>
    {
      var result = UtilsCanvas.GetLineThickness(5);

      result.Should().Be(5);
    });
  }

  [Fact]
  public void GetLineThickness_ReturnsOriginalValue()
  {
    StaThread.Run(() =>
    {
      var result = UtilsCanvas.GetLineThickness(10.5);

      result.Should().Be(10.5);
    });
  }

  #endregion
}

#endif
