using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas;

public partial class CanvasView : UserControl
{
  public CanvasView()
  {
    InitializeComponent();
    Loaded += CanvasView_Loaded;
  }

  public System.Windows.Controls.Canvas Canvas => MyCanvas;

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

  private void CanvasView_Loaded(object sender, RoutedEventArgs e)
  {
    var win = Window.GetWindow(this);
    (DataContext as CanvasViewModel)?.WindowLoadedCommand.Execute(
      new object[] { win, MyCanvas, MyZoomBorder });
  }

  private void ZoomBorder_MouseMove(object sender, MouseEventArgs e)
  {
    (DataContext as CanvasViewModel)?.MouseMoveCommand.Execute(e);
  }

  private void ZoomBorder_MouseDown(object sender, MouseButtonEventArgs e)
  {
    (DataContext as CanvasViewModel)?.MouseDownCommand.Execute(e);
  }

  private void ZoomBorder_MouseUp(object sender, MouseButtonEventArgs e)
  {
    (DataContext as CanvasViewModel)?.MouseUpCommand.Execute(e);

    if (e is { MiddleButton: MouseButtonState.Pressed, ClickCount: 2 })
      UtilsCanvas.ZoomToFit(MyCanvas, MyZoomBorder);
  }
}
