using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;
using AutoCADTools.Service._3D;
using AutoCADTools.Presentation._3D.Viewport3D.Selection;

namespace AutoCADTools.Presentation._3D.Viewport3D
{
  public class Viewport3DViewModel : BindableObject
  {
    private readonly IObjectManager3D _objectManager;
    private readonly ICameraService _cameraService;
    private readonly IViewportService _viewportService;
    private readonly SelectionService _selectionService;

    public Viewport3DViewModel(
      IObjectManager3D objectManager,
      ICameraService cameraService,
      IViewportService viewportService,
      SelectionService selectionService)
    {
      _objectManager = objectManager;
      _cameraService = cameraService;
      _viewportService = viewportService;
      _selectionService = selectionService;

      // Wire viewport service to camera service
      if (_viewportService is ViewportService vs)
        vs.CameraService = _cameraService;

      SceneRoot = new Model3DGroup();

      // Default lights
      var ambient = new AmbientLight(Color.FromArgb(255, 64, 64, 64));
      var directional = new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1));
      ambient.Freeze();
      directional.Freeze();
      SceneRoot.Children.Add(ambient);
      SceneRoot.Children.Add(directional);

      // Default grid
      AddDefaultScene();

      // Subscribe to events
      _objectManager.ObjectAdded += OnObjectAdded;
      _objectManager.ObjectRemoved += OnObjectRemoved;
      _objectManager.SelectionChanged += OnSelectionChanged;
      _objectManager.SceneCleared += OnSceneCleared;
      _cameraService.CameraChanged += OnCameraChanged;

      // Commands
      OrbitCommand = new RelayCommand(() => _viewportService.CurrentMode = EnumViewportMode.Orbit);
      PanCommand = new RelayCommand(() => _viewportService.CurrentMode = EnumViewportMode.Pan);
      SelectCommand = new RelayCommand(() => _viewportService.CurrentMode = EnumViewportMode.Select);
      ZoomInCommand = new RelayCommand(() => _cameraService.ZoomIn());
      ZoomOutCommand = new RelayCommand(() => _cameraService.ZoomOut());
      ZoomFitCommand = new RelayCommand(() => _cameraService.ZoomToFit(new Rect3D()));
      DeselectAllCommand = new RelayCommand(() => _objectManager.DeselectAll());
      ClearSceneCommand = new RelayCommand(() => _objectManager.Clear());

      PresetTopCommand = new RelayCommand(() => _cameraService.SetPresetView(EnumPresetView.Top));
      PresetFrontCommand = new RelayCommand(() => _cameraService.SetPresetView(EnumPresetView.Front));
      PresetLeftCommand = new RelayCommand(() => _cameraService.SetPresetView(EnumPresetView.Left));
      PresetIsoCommand = new RelayCommand(() => _cameraService.SetPresetView(EnumPresetView.Isometric));

      AddBoxCommand = new RelayCommand(() =>
      {
        var box = new Shapes3D.Box3D(1, 1, 1);
        box.Name = "Box_" + DateTime.Now.Ticks;
        _objectManager.Add(box);
      });

      AddPlaneCommand = new RelayCommand(() =>
      {
        var plane = new Shapes3D.Plane3D(20);
        plane.Name = "Plane_" + DateTime.Now.Ticks;
        _objectManager.Add(plane);
      });
    }

    private void AddDefaultScene()
    {
      var grid = new Shapes3D.Grid3D(20, 1);
      grid.Name = "Grid";
      _objectManager.Add(grid);

      var axis = new Shapes3D.Axis3D(10, 0.05);
      axis.Name = "Axis";
      _objectManager.Add(axis);
    }

    #region Scene Root

    public Model3DGroup SceneRoot { get; }

    private void OnObjectAdded(object sender, Core._3D.Events.Object3DEventArgs e)
    {
      Application.Current?.Dispatcher?.Invoke(() =>
      {
        SceneRoot.Children.Add(e.Object.WpfModel);
        _selectionService.RefreshOverlay();
      });
    }

    private void OnObjectRemoved(object sender, Core._3D.Events.Object3DEventArgs e)
    {
      Application.Current?.Dispatcher?.Invoke(() =>
      {
        SceneRoot.Children.Remove(e.Object.WpfModel);
        _selectionService.RefreshOverlay();
      });
    }

    private void OnSelectionChanged(object sender, Core._3D.Events.SelectionChangedEventArgs e)
    {
      Application.Current?.Dispatcher?.Invoke(() =>
      {
        _selectionService.RefreshOverlay();
        SelectedObjects.Clear();
        foreach (var obj in _objectManager.SelectedObjects)
          SelectedObjects.Add(obj);
      });
    }

    private void OnSceneCleared(object sender, EventArgs e)
    {
      Application.Current?.Dispatcher?.Invoke(() =>
      {
        // Keep ambient + directional lights
        while (SceneRoot.Children.Count > 2)
          SceneRoot.Children.RemoveAt(2);
        _selectionService.RefreshOverlay();
        SelectedObjects.Clear();
      });
    }

    private void OnCameraChanged(object sender, EventArgs e)
    {
      OnPropertyChanged(nameof(CameraService));
    }

    #endregion

    #region Properties

    public ICameraService CameraService => _cameraService;
    public IViewportService ViewportService => _viewportService;
    public IObjectManager3D ObjectManager => _objectManager;
    public SelectionService SelectionServiceInstance => _selectionService;

    public ObservableCollection<IObject3DBase> SelectedObjects { get; } = new ObservableCollection<IObject3DBase>();

    [ChangeTracker]
    public EnumViewportMode CurrentToolMode
    {
      get => _viewportService.CurrentMode;
      set => _viewportService.CurrentMode = value;
    }

    [ChangeTracker]
    public bool IsGridVisible
    {
      get => _isGridVisible;
      set
      {
        if (SetProperty(ref _isGridVisible, value))
          UpdateGridVisibility();
      }
    }
    private bool _isGridVisible = true;

    [ChangeTracker]
    public bool ShowAxes
    {
      get => _showAxes;
      set
      {
        if (SetProperty(ref _showAxes, value))
          UpdateAxisVisibility();
      }
    }
    private bool _showAxes = true;

    #endregion

    #region Visibility

    private void UpdateGridVisibility()
    {
      foreach (var child in SceneRoot.Children)
      {
        if (child is Model3DGroup group)
        {
          // Find grid by name... simple approach: just toggle all non-light children
        }
      }
    }

    private void UpdateAxisVisibility()
    {
      // Similar logic
    }

    #endregion

    #region Commands

    public ICommand OrbitCommand { get; }
    public ICommand PanCommand { get; }
    public ICommand SelectCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ZoomFitCommand { get; }
    public ICommand DeselectAllCommand { get; }
    public ICommand ClearSceneCommand { get; }
    public ICommand PresetViewCommand { get; } = null!;
    public ICommand PresetTopCommand { get; }
    public ICommand PresetFrontCommand { get; }
    public ICommand PresetLeftCommand { get; }
    public ICommand PresetIsoCommand { get; }
    public ICommand AddBoxCommand { get; }
    public ICommand AddPlaneCommand { get; }

    #endregion
  }
}
