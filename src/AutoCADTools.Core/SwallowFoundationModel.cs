namespace AutoCADTools.Core;

public enum ColumnShape
{
  Rectangular = 0,
  Circular = 1
}

public class SwallowFoundationModel
{
  private bool _isRectangularColumn = true;
  private bool _isCircularColumn = false;
  private bool _drawColumnRebar = false;
  private bool _isAlternateRebarSpacing = false;
  private double _scale = 3;

  public bool IsRectangularColumn
  {
    get => _isRectangularColumn;
    set
    {
      _isRectangularColumn = value;
      if (value)
      {
        _isCircularColumn = false;
      }
    }
  }

  public bool IsCircularColumn
  {
    get => _isCircularColumn;
    set
    {
      _isCircularColumn = value;
      if (value)
      {
        _isRectangularColumn = false;
      }
    }
  }

  public ColumnShape Shape => IsRectangularColumn ? ColumnShape.Rectangular : ColumnShape.Circular;

  /// <summary>Footing length in X direction (mm).</summary>
  public double LengthX { get; set; }

  /// <summary>Footing length in Y direction (mm).</summary>
  public double LengthY { get; set; }

  /// <summary>Column axis offset in X direction (mm).</summary>
  public double AxisOffsetX { get; set; }

  /// <summary>Column axis offset in Y direction (mm).</summary>
  public double AxisOffsetY { get; set; }

  /// <summary>Step height at column base (mm).</summary>
  public double StepHeightH1 { get; set; }

  /// <summary>Step height at footing base (mm).</summary>
  public double StepHeightH2 { get; set; }

  /// <summary>Foundation bottom elevation (mm).</summary>
  public double FoundationBottomLevel { get; set; }

  /// <summary>Floor elevation (mm).</summary>
  public double FloorLevel { get; set; }

  /// <summary>Floor elevation T1 (mm).</summary>
  public double FloorLevelT1 { get; set; }

  /// <summary>Concrete pad width / bleed area around footing (mm).</summary>
  public double ConcretePadWidth { get; set; }

  /// <summary>Concrete pad thickness (mm).</summary>
  public double ConcretePadThickness { get; set; }

  /// <summary>Column width in X direction (mm).</summary>
  public double ColumnWidthX { get; set; }

  /// <summary>Column width in Y direction (mm).</summary>
  public double ColumnWidthY { get; set; }

  /// <summary>Column position X offset (mm).</summary>
  public double ColumnPositionX { get; set; }

  /// <summary>Column position Y offset (mm).</summary>
  public double ColumnPositionY { get; set; }

  /// <summary>Rebar in X direction, e.g. "2a150" = 2 rebars @ 150mm spacing.</summary>
  public string RebarX { get; set; } = "";

  /// <summary>Rebar in Y direction, e.g. "2a150" = 2 rebars @ 150mm spacing.</summary>
  public string RebarY { get; set; } = "";

  public bool DrawColumnRebar
  {
    get => _drawColumnRebar;
    set => _drawColumnRebar = value;
  }

  public bool IsAlternateRebarSpacing
  {
    get => _isAlternateRebarSpacing;
    set => _isAlternateRebarSpacing = value;
  }

  /// <summary>Column rebar, e.g. "2d25" = phi2 @ 25mm.</summary>
  public string ColumnRebar { get; set; } = "";

  /// <summary>Stirrup rebar, e.g. "8a150" = phi8 @ 150mm.</summary>
  public string StirrupRebar { get; set; } = "";

  public double Scale
  {
    get => _scale;
    set => _scale = value;
  }

  /// <summary>Number of column rebars along X side.</summary>
  public int ColumnRebarCountX { get; set; }

  /// <summary>Number of column rebars along Y side.</summary>
  public int ColumnRebarCountY { get; set; }

  public bool IsPrimaryDirection { get; set; }

  /// <summary>Inverse of IsPrimaryDirection, matching VB behaviour.</summary>
  public bool IsSecondaryDirection => !IsPrimaryDirection;

  public string FoundationName { get; set; } = "";
  public int Quantity { get; set; }
  public double Cover { get; set; }

  /// <summary>Lap splice length for column rebars (mm).</summary>
  public double LapLength { get; set; }

  public SwallowFoundationModel()
  {
  }

  public SwallowFoundationModel(
      string foundationName,
      int quantity,
      double scale,
      double lengthX,
      double lengthY,
      double axisOffsetX,
      double axisOffsetY,
      double stepHeightH1,
      double stepHeightH2,
      double foundationBottomLevel,
      double floorLevel,
      double floorLevelT1,
      double concretePadWidth,
      double concretePadThickness,
      bool isRectangularColumn,
      double columnWidthX,
      double columnWidthY,
      double columnPositionX,
      double columnPositionY,
      string rebarX,
      string rebarY,
      bool drawColumnRebar,
      bool isAlternateRebarSpacing,
      string columnRebar,
      string stirrupRebar,
      bool isPrimaryDirection,
      double cover,
      int columnRebarCountX,
      int columnRebarCountY,
      double lapLength)
  {
    FoundationName = foundationName;
    Quantity = quantity;
    Scale = scale;
    LengthX = lengthX;
    LengthY = lengthY;
    AxisOffsetX = axisOffsetX;
    AxisOffsetY = axisOffsetY;
    StepHeightH1 = stepHeightH1;
    StepHeightH2 = stepHeightH2;
    FoundationBottomLevel = foundationBottomLevel;
    FloorLevel = floorLevel;
    FloorLevelT1 = floorLevelT1;
    ConcretePadWidth = concretePadWidth;
    ConcretePadThickness = concretePadThickness;
    IsRectangularColumn = isRectangularColumn;
    IsCircularColumn = !isRectangularColumn;
    ColumnWidthX = columnWidthX;
    ColumnWidthY = columnWidthY;
    ColumnPositionX = columnPositionX;
    ColumnPositionY = columnPositionY;
    RebarX = rebarX;
    RebarY = rebarY;
    DrawColumnRebar = drawColumnRebar;
    IsAlternateRebarSpacing = isAlternateRebarSpacing;
    ColumnRebar = columnRebar;
    StirrupRebar = stirrupRebar;
    IsPrimaryDirection = isPrimaryDirection;
    Cover = cover;
    ColumnRebarCountX = columnRebarCountX;
    ColumnRebarCountY = columnRebarCountY;
    LapLength = lapLength;
  }
}
