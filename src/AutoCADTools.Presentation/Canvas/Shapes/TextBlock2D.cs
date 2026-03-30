using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public class TextBlock2D
{
  private readonly TextBlock? _textBlock;
  public TextBlock2D(
    System.Windows.Controls.Canvas? canvas,
    string text,
    double textFontSize,
    Point position,
    Brush? textColor,
    double angle = 0,
    double margin = 0,
    EnumTextAlignment textAlignment = EnumTextAlignment.CenterMiddle,
    int zIndex = 0)
  {
    if (canvas == null)
    {
      return;
    }

    if (string.IsNullOrEmpty(text))
    {
      return;
    }

    if (textFontSize < 1e-9)
    {
      return;
    }

    if (!position.IsValid())
    {
      return;
    }

    if (textColor == null)
    {
      return;
    }

    var tb = new TextBlock {
      Text = text,
      FontSize = textFontSize,
      LineHeight = textFontSize,
      LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
      Foreground = textColor,
      Margin = new Thickness(margin),
      FontFamily = new FontFamily("Arial")
    };
    tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    var textSize = tb.DesiredSize;

    if (textSize.Width < 1e-9) textSize.Width = 1;
    if (textSize.Height < 1e-9) textSize.Height = 1;

    var actualAngle = angle;
    var actualAlignment = textAlignment;

    switch (angle)
    {
      case >= 90:
        actualAngle = angle - 180;
        actualAlignment = textAlignment switch
        {
          EnumTextAlignment.TopLeft => EnumTextAlignment.BottomRight,
          EnumTextAlignment.TopMiddle => EnumTextAlignment.BottomMiddle,
          EnumTextAlignment.TopRight => EnumTextAlignment.BottomLeft,
          EnumTextAlignment.CenterLeft => EnumTextAlignment.CenterRight,
          EnumTextAlignment.CenterMiddle => EnumTextAlignment.CenterMiddle,
          EnumTextAlignment.CenterRight => EnumTextAlignment.CenterLeft,
          EnumTextAlignment.BottomLeft => EnumTextAlignment.TopRight,
          EnumTextAlignment.BottomMiddle => EnumTextAlignment.TopMiddle,
          EnumTextAlignment.BottomRight => EnumTextAlignment.TopLeft,
          _ => textAlignment
        };
        break;
      case < -90:
        actualAngle = angle + 180;
        actualAlignment = textAlignment switch
        {
          EnumTextAlignment.TopLeft => EnumTextAlignment.BottomRight,
          EnumTextAlignment.TopMiddle => EnumTextAlignment.BottomMiddle,
          EnumTextAlignment.TopRight => EnumTextAlignment.BottomLeft,
          EnumTextAlignment.CenterLeft => EnumTextAlignment.CenterRight,
          EnumTextAlignment.CenterMiddle => EnumTextAlignment.CenterMiddle,
          EnumTextAlignment.CenterRight => EnumTextAlignment.CenterLeft,
          EnumTextAlignment.BottomLeft => EnumTextAlignment.TopRight,
          EnumTextAlignment.BottomMiddle => EnumTextAlignment.TopMiddle,
          EnumTextAlignment.BottomRight => EnumTextAlignment.TopLeft,
          _ => textAlignment
        };
        break;
    }

    var pivotX = actualAlignment switch
    {
      EnumTextAlignment.TopLeft or EnumTextAlignment.CenterLeft or EnumTextAlignment.BottomLeft => 0,
      EnumTextAlignment.TopMiddle or EnumTextAlignment.CenterMiddle or EnumTextAlignment.BottomMiddle => textSize.Width / 2,
      EnumTextAlignment.TopRight or EnumTextAlignment.CenterRight or EnumTextAlignment.BottomRight => textSize.Width,
      _ => textSize.Width / 2
    };

    var pivotY = actualAlignment switch
    {
      EnumTextAlignment.TopLeft or EnumTextAlignment.TopMiddle or EnumTextAlignment.TopRight => 0,
      EnumTextAlignment.CenterLeft or EnumTextAlignment.CenterMiddle or EnumTextAlignment.CenterRight => textSize.Height / 2,
      EnumTextAlignment.BottomLeft or EnumTextAlignment.BottomMiddle or EnumTextAlignment.BottomRight => textSize.Height,
      _ => textSize.Height / 2
    };

    var transformGroup = new TransformGroup();
    transformGroup.Children.Add(new RotateTransform(actualAngle, pivotX, pivotY));
    transformGroup.Children.Add(new TranslateTransform(-pivotX, -pivotY));

    tb.RenderTransform = transformGroup;

    System.Windows.Controls.Canvas.SetLeft(tb, position.X);
    System.Windows.Controls.Canvas.SetTop(tb, position.Y);
    System.Windows.Controls.Canvas.SetZIndex(tb, zIndex);
    canvas.Children.Add(tb);
    _textBlock = tb;
  }

  public double Height => _textBlock?.ActualHeight ?? 0;
}
