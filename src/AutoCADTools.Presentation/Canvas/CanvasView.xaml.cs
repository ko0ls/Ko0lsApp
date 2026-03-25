using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoCADTools.Presentation.Canvas;

public partial class CanvasView : UserControl
{
  public CanvasView()
  {
    InitializeComponent();
    Loaded += CanvasView_Loaded;
    MyZoomBorder.MouseMove += ZoomBorder_MouseMove;
    MyZoomBorder.MouseDown += ZoomBorder_MouseDown;
    MyZoomBorder.MouseUp += ZoomBorder_MouseUp;
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

  private void ZoomBorder_MouseMove(object sender, MouseEventArgs e)
  {
    // MouseMoveCommand is a no-op stub; nothing to do here
  }

  private void ZoomBorder_MouseDown(object sender, MouseButtonEventArgs e)
  {
    (DataContext as CanvasViewModel)?.MouseDownCommand.Execute(e);
  }

  private void ZoomBorder_MouseUp(object sender, MouseButtonEventArgs e)
  {
    (DataContext as CanvasViewModel)?.MouseUpCommand.Execute(e);
  }

  private void CanvasView_Loaded(object sender, RoutedEventArgs e)
  {
    var win = Window.GetWindow(this);
    (DataContext as CanvasViewModel)?.WindowLoadedCommand.Execute(
      new object[] { win, MyCanvas, MyZoomBorder });
  }
}
