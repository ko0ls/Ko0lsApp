using Newtonsoft.Json;

namespace AutoCADTools.Presentation.Utils;

public static class ObjectCloner
{
  private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
  };

  public static T? Clone<T>(this T? source)
  {
    if (source == null) return default;
    var json = JsonConvert.SerializeObject(source, Settings);
    return JsonConvert.DeserializeObject<T>(json);
  }

  public static object? Clone(this object? source)
  {
    if (source == null) return null;
    var json = JsonConvert.SerializeObject(source, Settings);
    return JsonConvert.DeserializeObject(json, source.GetType());
  }
}
