using System;
using System.Windows.Input;
using AutoCADTools.Core;
using AutoCADTools.Presentation.Drawing;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Validation;

namespace AutoCADTools.Presentation.ViewModels;

public class SwallowFoundationViewModel : BindableObject
{
  private readonly SwallowFoundationPreviewDrawer? _previewDrawer;

  // Drawing scale
  private double _scale = 3;
  // Foundation dimensions
  private double _lengthX = 1500;
  private double _lengthY = 1800;
  private double _stepHeightH1 = 500;
  // Step heights (second step)
  private double _stepHeightH2 = 200;
  // Concrete leveling pad
  private double _concretePadWidth = 100;
  private double _concretePadThickness = 100;
  // Column neck dimensions
  private double _columnWidthX = 220;
  private double _columnWidthY = 220;
  // Rebar / concrete cover
  private double _cover = 30;
  // Identity
  private string _foundationName = "M1";
  private int _quantity = 1;
  private double _axisOffsetX = 750;
  private double _axisOffsetY = 900;
  private double _foundationBottomLevel = -1500;
  private double _floorLevel = -450;
  private double _floorLevelT1 = -50;
  private bool _isRectangularColumn = true;
  private double _columnPositionX = 750;
  private double _columnPositionY = 900;
  private string _rebarX = "d10a200";
  private string _rebarY = "d10a200";
  private bool _drawColumnRebar;
  private bool _isAlternateRebarSpacing;
  private string _columnRebar = "d16";
  private string _stirrupRebar = "d6a100";
  private bool _isPrimaryDirection = true;
  private int _columnRebarCountX = 2;
  private int _columnRebarCountY = 2;
  private double _lapLength = 40;

  public SwallowFoundationViewModel(SwallowFoundationPreviewDrawer previewDrawer)
  {
    _previewDrawer = previewDrawer;
    OKCommand = new RelayCommand(OnOK, () => !HasErrors);
    CancelCommand = new RelayCommand(OnCancel);
    PropertyChanged += OnPropertyChangedForPreview;
    RefreshPreview();
  }

  private void OnPropertyChangedForPreview(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
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
  public double AxisOffsetX
  {
    get => _axisOffsetX;
    set => SetProperty(ref _axisOffsetX, value);
  }

  [CannotNull]
  public double AxisOffsetY
  {
    get => _axisOffsetY;
    set => SetProperty(ref _axisOffsetY, value);
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
  [GreaterThanOrEqual(0)]
  public double FoundationBottomLevel
  {
    get => _foundationBottomLevel;
    set => SetProperty(ref _foundationBottomLevel, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double FloorLevel
  {
    get => _floorLevel;
    set => SetProperty(ref _floorLevel, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double FloorLevelT1
  {
    get => _floorLevelT1;
    set => SetProperty(ref _floorLevelT1, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double ConcretePadWidth
  {
    get => _concretePadWidth;
    set => SetProperty(ref _concretePadWidth, value);
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
  public double ColumnPositionX
  {
    get => _columnPositionX;
    set => SetProperty(ref _columnPositionX, value);
  }

  [CannotNull]
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

  public bool IsAlternateRebarSpacing
  {
    get => _isAlternateRebarSpacing;
    set => SetProperty(ref _isAlternateRebarSpacing, value);
  }

  [NotEmpty]
  public string ColumnRebar
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

  public bool IsPrimaryDirection
  {
    get => _isPrimaryDirection;
    set => SetProperty(ref _isPrimaryDirection, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double Cover
  {
    get => _cover;
    set => SetProperty(ref _cover, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(1)]
  public int ColumnRebarCountX
  {
    get => _columnRebarCountX;
    set => SetProperty(ref _columnRebarCountX, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(1)]
  public int ColumnRebarCountY
  {
    get => _columnRebarCountY;
    set => SetProperty(ref _columnRebarCountY, value);
  }

  [CannotNull]
  [GreaterThanOrEqual(0)]
  public double LapLength
  {
    get => _lapLength;
    set => SetProperty(ref _lapLength, value);
  }

  public ICommand OKCommand { get; }
  public ICommand CancelCommand { get; }

  public event Action? CloseRequested;

  public void RefreshPreview()
  {
    if (_previewDrawer == null) return;
    var model = ToModel();
    _previewDrawer.RefreshDrawing(model);
  }

  public SwallowFoundationModel ToModel()
  {
    return new SwallowFoundationModel(
      FoundationName, Quantity, Scale,
      LengthX, LengthY, AxisOffsetX, AxisOffsetY,
      StepHeightH1, StepHeightH2,
      FoundationBottomLevel, FloorLevel, FloorLevelT1,
      ConcretePadWidth, ConcretePadThickness,
      IsRectangularColumn,
      ColumnWidthX, ColumnWidthY,
      ColumnPositionX, ColumnPositionY,
      RebarX, RebarY,
      DrawColumnRebar, IsAlternateRebarSpacing,
      ColumnRebar, StirrupRebar,
      IsPrimaryDirection, Cover,
      ColumnRebarCountX, ColumnRebarCountY,
      LapLength);
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
