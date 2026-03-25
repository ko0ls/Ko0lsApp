using System;
using System.Windows;
using System.Windows.Controls;
using AutoCADTools.Presentation.Canvas;

namespace AutoCADTools.Presentation._3D
{
  public partial class MainView : Window
  {
    private bool _canvasViewLoaded;

    public MainView(object dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
      MainTabControl.SelectionChanged += OnTabSelectionChanged;
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!Equals(e.Source, MainTabControl)) return;
      if (MainTabControl.SelectedItem == CanvasTab && !_canvasViewLoaded)
      {
        _canvasViewLoaded = true;
        var canvasView = new CanvasView
        {
          DataContext = ((MainViewModel)DataContext).CanvasViewModel
        };
        CanvasTab.Content = canvasView;
      }
    }
  }
}
