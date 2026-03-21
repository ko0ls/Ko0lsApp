namespace AutoCADTools.Presentation.Utils.HelperTracking
{
  public interface ITrackedChange
  {
    string PropertyName { get; }
    object? OriginalValue { get; }
    object? CurrentValue { get; }
  }
}