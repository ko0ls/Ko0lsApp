using System.Windows;
using System.Windows.Controls;

namespace AutoCADTools.Presentation.Canvas;

public partial class CanvasView : UserControl
{
  public CanvasView()
  {
    InitializeComponent();
  }

  public bool ShowDimCheckBox
  {
    get => (bool)GetValue(ShowDimCheckBoxProperty);
    set => SetValue(ShowDimCheckBoxProperty, value);
  }

  public static readonly DependencyProperty ShowDimCheckBoxProperty =
    DependencyProperty.Register(
      nameof(ShowDimCheckBox),
      typeof(bool),
      typeof(CanvasView),
      new PropertyMetadata(false));
}
