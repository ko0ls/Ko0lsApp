namespace AutoCADTools.Presentation.Utils
{
    public class ModelPropertyChangedEventArgs : System.EventArgs
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? Value { get; set; }
        public object? OldValue { get; set; }
    }
}
