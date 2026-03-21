using System.ComponentModel;

namespace AutoCADTools.Presentation.Utils.HelperTracking
{
    public class PropertyChangedTrackedEventArgs : PropertyChangedEventArgs
    {
        public object? OriginalValue { get; private set; }
        public object? CurrentValue { get; private set; }

        public PropertyChangedTrackedEventArgs(string? propertyName, object? originalValue, object? currentValue)
            : base(propertyName)
        {
            OriginalValue = originalValue;
            CurrentValue = currentValue;
        }
    }
}
