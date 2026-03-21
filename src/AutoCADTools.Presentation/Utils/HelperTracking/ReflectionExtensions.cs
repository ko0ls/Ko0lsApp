using System;

namespace AutoCADTools.Presentation.Utils.HelperTracking
{
  public static class ReflectionExtensions
  {
    public static bool IsNullable(this Type type, out Type? underlyingType)
    {
      underlyingType = Nullable.GetUnderlyingType(type);
      return underlyingType != null;
    }

    public static bool IsNullable(this Type type)
    {
      var underlyingType = Nullable.GetUnderlyingType(type);
      return underlyingType != null;
    }
  }
}