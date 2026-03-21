using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AutoCADTools.Presentation.Utils
{
  public static class ObjectCloner
  {
    public static object? Clone(this object? source)
    {
      if (source == null) return null!;

      using var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, source);
      stream.Position = 0;
      return formatter.Deserialize(stream)!;
    }
  }
}