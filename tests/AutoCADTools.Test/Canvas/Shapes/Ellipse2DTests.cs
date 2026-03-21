#if NET8_0_OR_GREATER
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class Ellipse2DTests : IDisposable
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
      var act = () => new Ellipse2D(null, new Point(5, 5), 10, 10, 2, Brushes.Black);

      act.Should().NotThrow();
    });
  }

  [Fact]
  public void Constructor_WithInvalidCenter_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(double.NaN, 5), 10, 10, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithZeroWidth_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(5, 5), 0, 10, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithZeroHeight_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(5, 5), 10, 0, 2, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithZeroThickness_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 0, Brushes.Black);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  [Fact]
  public void Constructor_WithNullColor_DoesNotAddToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, null);

      _canvas.Children.Count.Should().Be(0);
    });
  }

  #endregion

  #region Constructor Valid Cases

  [Fact]
  public void Constructor_WithValidParams_AddsEllipseToCanvas()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();

      new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      _canvas.Children.Count.Should().Be(1);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsCorrectDimensions()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 20, 30, 2, Brushes.Blue);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Width.Should().Be(20);
      ellipse.Height.Should().Be(30);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_PositionsAtCenter()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var center = new Point(10, 10);
      var width = 20.0;
      var height = 30.0;
      var ellipse2D = new Ellipse2D(_canvas, center, width, height, 2, Brushes.Blue);

      var ellipse = ellipse2D.GetEllipse();
      Canvas.GetLeft(ellipse).Should().Be(center.X - width / 2);
      Canvas.GetTop(ellipse).Should().Be(center.Y - height / 2);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 3, Brushes.Blue);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.StrokeThickness.Should().Be(3);
    });
  }

  [Fact]
  public void Constructor_WithValidParams_SetsStrokeColor()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Green);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Stroke.Should().Be(Brushes.Green);
    });
  }

  [Fact]
  public void Constructor_WithFillColor_SetsFill()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue, Brushes.Yellow);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Fill.Should().Be(Brushes.Yellow);
    });
  }

  [Fact]
  public void Constructor_WithoutFillColor_SetsTransparentFill()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Fill.Should().Be(Brushes.Transparent);
    });
  }

  [Fact]
  public void Constructor_WithAngle_SetsRotateTransform()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue, angle: 45);

      var ellipse = ellipse2D.GetEllipse();
      ellipse.RenderTransform.Should().BeOfType<RotateTransform>();
    });
  }

  [Fact]
  public void Constructor_WithZeroAngle_DoesNotSetRotateTransform()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue, angle: 0);

      var ellipse = ellipse2D.GetEllipse();
      // WPF defaults RenderTransform to Transform.Identity, not null
      ellipse.RenderTransform.Should().Be(Transform.Identity);
    });
  }

  #endregion

  #region MakeHighLight

  [Fact]
  public void MakeHighLight_SetsRedStroke()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      ellipse2D.MakeHighLight();

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Stroke.Should().Be(Brushes.Red);
    });
  }

  [Fact]
  public void MakeHighLight_IncreasesStrokeThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 2.0;
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, originalThickness, Brushes.Blue);

      ellipse2D.MakeHighLight();

      var ellipse = ellipse2D.GetEllipse();
      ellipse.StrokeThickness.Should().Be(5 * originalThickness);
    });
  }

  [Fact]
  public void MakeHighLight_SetsZIndexToOne()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      ellipse2D.MakeHighLight();

      Canvas.GetZIndex(ellipse2D.GetEllipse()).Should().Be(1);
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
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      ellipse2D.MakeHighLight();
      ellipse2D.ResetHighLight();

      var ellipse = ellipse2D.GetEllipse();
      ellipse.Stroke.Should().Be(Brushes.Blue);
    });
  }

  [Fact]
  public void ResetHighLight_RestoresOriginalThickness()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var originalThickness = 3.0;
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, originalThickness, Brushes.Blue);

      ellipse2D.MakeHighLight();
      ellipse2D.ResetHighLight();

      var ellipse = ellipse2D.GetEllipse();
      ellipse.StrokeThickness.Should().Be(originalThickness);
    });
  }

  [Fact]
  public void ResetHighLight_ResetsZIndexToZero()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      ellipse2D.MakeHighLight();
      ellipse2D.ResetHighLight();

      Canvas.GetZIndex(ellipse2D.GetEllipse()).Should().Be(0);
    });
  }

  #endregion

  #region CheckMoveOver

  [Fact]
  public void CheckMoveOver_WithPointOnEllipse_ReturnsTrue()
  {
    bool result = false;
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);
      result = ellipse2D.CheckMoveOver(new Point(5, 5));
    });
    result.Should().BeTrue();
  }

  [Fact]
  public void CheckMoveOver_WithPointFarFromEllipse_ReturnsFalse()
  {
    StaThread.Run(() =>
    {
      _canvas = new Canvas();
      var ellipse2D = new Ellipse2D(_canvas, new Point(5, 5), 10, 10, 2, Brushes.Blue);

      var result = ellipse2D.CheckMoveOver(new Point(100, 100));

      result.Should().BeFalse();
    });
  }

  #endregion
}

#endif
