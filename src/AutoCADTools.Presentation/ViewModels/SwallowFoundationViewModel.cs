using System;
using System.Collections.Generic;
using System.Windows.Input;
using AutoCADTools.Core;
using AutoCADTools.Presentation.Drawing;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Validation;

namespace AutoCADTools.Presentation.ViewModels;

public class SwallowFoundationViewModel : BindableObject
{
  private readonly SwallowFoundationPreviewDrawer _previewDrawer;
  private readonly Action _onRefresh;
  private int _scaleCanvas = 100;

  public int ScaleCanvas
  {
    get => _scaleCanvas;
    set => SetProperty(ref _scaleCanvas, value);
  }

  // Drawing scale (e.g. 1:3)
  private double _scale = 3;
  // Foundation dimensions (mm)
  private double _lengthX = 1500;                     // Footing width (X direction)
  private double _lengthY = 1800;                      // Footing length (Y direction)
  private double _stepHeightH1 = 500;                  // Maximum footing thickness (includes chamfer), mm
  // Step heights
  private double _stepHeightH2 = 200;                  // Non-chamfered footing thickness, mm → chamfer = H1-H2
  // Concrete leveling pad
  private double _concretePadExtension = 100;         // Concrete pad extension beyond footing Lx/Ly, mm
  private double _concretePadThickness = 100;          // Concrete leveling pad thickness, mm
  // Column neck dimensions
  private double _columnWidthX = 220;
  private double _columnWidthY = 220;
  // Rebar / concrete cover
  private double _cover = 30;
  // Identity
  private string _foundationName = "M1";
  private int _quantity = 1;
  private double _axisPositionX = 750;
  private double _axisPositionY = 900;
  private double _foundationBottomLevel = -1500;
  private double _groundLevel = -450;
  private double _floorLevel1 = -50;
  private bool _isRectangularColumn = true;
  private double _columnPositionX = 750;
  private double _columnPositionY = 900;
  private string _rebarX = "d10a200";
  private string _rebarY = "d10a200";
  // Available rebar diameters (mm)
  public IEnumerable<int> ColumnRebarOptions { get; } = new[]
  {
    6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 30, 32, 36, 40
  };

  // Available drawing scale options (denominator: 1:N)
  public IEnumerable<int> ScaleOptions { get; } = new[] { 10, 20, 50, 100, 200, 500 };

  private bool _drawColumnRebar;
  private bool _isStaggeredLayout;
  private int _columnRebar = 16;
  private string _stirrupRebar = "d6a100";
  // Primary rebar direction: "X" or "Y" (X = rebar along X is main/bottom layer)
  private string _primaryDirection = "X";
  private int _columnRebarCountX = 2;
  private int _columnRebarCountY = 2;
  private double _lapSpliceLength = 40;

  public SwallowFoundationPreviewViewModel PreviewCanvasViewModel { get; }

  public SwallowFoundationViewModel(SwallowFoundationPreviewDrawer previewDrawer)
  {
    _previewDrawer = previewDrawer;
    _onRefresh = RefreshPreview;
    PreviewCanvasViewModel = new SwallowFoundationPreviewViewModel(_onRefresh);
    OKCommand = new RelayCommand(OnOK, () => !HasErrors);
    CancelCommand = new RelayCommand(OnCancel);
    PropertyChanged += OnPropertyChangedForPreview;
  }

  private void OnPropertyChangedForPreview(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(ScaleCanvas))
      _previewDrawer.RefreshDrawing(ToModel(), ScaleCanvas);
    else
      RefreshPreview();
  }

  [NotEmpty]
  public string FoundationName
  {
    get => _foundationName;
    set => SetProperty(ref _foundationName, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(1)]
  public int Quantity
  {
    get => _quantity;
    set => SetProperty(ref _quantity, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double Scale
  {
    get => _scale;
    set => SetProperty(ref _scale, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double LengthX
  {
    get => _lengthX;
    set => SetProperty(ref _lengthX, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double LengthY
  {
    get => _lengthY;
    set => SetProperty(ref _lengthY, value);
  }

  [CannotNull]
  public double AxisPositionX
  {
    get => _axisPositionX;
    set => SetProperty(ref _axisPositionX, value);
  }

  [CannotNull]
  public double AxisPositionY
  {
    get => _axisPositionY;
    set => SetProperty(ref _axisPositionY, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double StepHeightH1
  {
    get => _stepHeightH1;
    set => SetProperty(ref _stepHeightH1, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double StepHeightH2
  {
    get => _stepHeightH2;
    set => SetProperty(ref _stepHeightH2, value);
  }

  [CannotNull]
  public double FoundationBottomLevel
  {
    get => _foundationBottomLevel;
    set => SetProperty(ref _foundationBottomLevel, value);
  }

  [CannotNull]
  public double GroundLevel
  {
    get => _groundLevel;
    set => SetProperty(ref _groundLevel, value);
  }

  [CannotNull]
  public double FloorLevel1
  {
    get => _floorLevel1;
    set => SetProperty(ref _floorLevel1, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double ConcretePadExtension
  {
    get => _concretePadExtension;
    set => SetProperty(ref _concretePadExtension, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double ConcretePadThickness
  {
    get => _concretePadThickness;
    set => SetProperty(ref _concretePadThickness, value);
  }

  public bool IsRectangularColumn
  {
    get => _isRectangularColumn;
    set => SetProperty(ref _isRectangularColumn, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double ColumnWidthX
  {
    get => _columnWidthX;
    set => SetProperty(ref _columnWidthX, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double ColumnWidthY
  {
    get => _columnWidthY;
    set => SetProperty(ref _columnWidthY, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double ColumnPositionX
  {
    get => _columnPositionX;
    set => SetProperty(ref _columnPositionX, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double ColumnPositionY
  {
    get => _columnPositionY;
    set => SetProperty(ref _columnPositionY, value);
  }

  [NotEmpty]
  public string RebarX
  {
    get => _rebarX;
    set => SetProperty(ref _rebarX, value);
  }

  [NotEmpty]
  public string RebarY
  {
    get => _rebarY;
    set => SetProperty(ref _rebarY, value);
  }

  public bool DrawColumnRebar
  {
    get => _drawColumnRebar;
    set => SetProperty(ref _drawColumnRebar, value);
  }

  public bool IsStaggeredLayout
  {
    get => _isStaggeredLayout;
    set => SetProperty(ref _isStaggeredLayout, value);
  }

  public int ColumnRebar
  {
    get => _columnRebar;
    set => SetProperty(ref _columnRebar, value);
  }

  [NotEmpty]
  public string StirrupRebar
  {
    get => _stirrupRebar;
    set => SetProperty(ref _stirrupRebar, value);
  }

  public string PrimaryDirection
  {
    get => _primaryDirection;
    set => SetProperty(ref _primaryDirection, value);
  }

  // Computed: true if primary direction is X, false if Y
  public bool IsPrimaryDirection => _primaryDirection == "X";

  [CannotNull]
  [GreaterThan(0)]
  public double Cover
  {
    get => _cover;
    set => SetProperty(ref _cover, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(2)]
  public int ColumnRebarCountX
  {
    get => _columnRebarCountX;
    set => SetProperty(ref _columnRebarCountX, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(2)]
  public int ColumnRebarCountY
  {
    get => _columnRebarCountY;
    set => SetProperty(ref _columnRebarCountY, value);
  }

  [CannotNull]
  [GreaterThan(0)]
  public double LapSpliceLength
  {
    get => _lapSpliceLength;
    set => SetProperty(ref _lapSpliceLength, value);
  }

  public ICommand OKCommand { get; }
  public ICommand CancelCommand { get; }

  public event Action? CloseRequested;

  public void RefreshPreview()
  {
    var model = ToModel();
    _previewDrawer.RefreshDrawing(model);
  }

  public SwallowFoundationModel ToModel()
  {
    return new SwallowFoundationModel(
      FoundationName, Quantity, Scale,
      LengthX, LengthY, AxisPositionX, AxisPositionY,
      StepHeightH1, StepHeightH2,
      FoundationBottomLevel, GroundLevel, FloorLevel1,
      ConcretePadExtension, ConcretePadThickness,
      IsRectangularColumn,
      ColumnWidthX, ColumnWidthY,
      ColumnPositionX, ColumnPositionY,
      RebarX, RebarY,
      DrawColumnRebar, IsStaggeredLayout,
      ColumnRebar, StirrupRebar,
      IsPrimaryDirection, Cover,
      ColumnRebarCountX, ColumnRebarCountY,
      LapSpliceLength);
  }

  private void OnOK()
  {
    // TODO: call AutoCAD drawer service (step 5)
    CloseRequested?.Invoke();
  }

  private void OnCancel()
  {
    CloseRequested?.Invoke();
  }
}
