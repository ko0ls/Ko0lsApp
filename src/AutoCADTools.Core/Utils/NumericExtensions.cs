using System;

namespace AutoCADTools.Core.Utils ;

public static class NumericExtensions
{
  private const double Torlerance = 1e-4 ;

  extension(double a)
  {
    public bool IsZero(double tolerance = Torlerance )
    {
      return IsAlmostEqualsTo( 0.0, a, tolerance ) ;
    }

    public bool IsAlmostEqualsTo(double b, double tolerance = Torlerance )
    {
      return Math.Abs( b - a ) < tolerance ;
    }

    public bool IsSmaller(double b, double tolerance = Torlerance )
    {
      return a + tolerance < b ;
    }

    public bool IsSmallerEqual(double b, double tolerance = Torlerance )
    {
      if ( a + tolerance >= b ) {
        return Math.Abs( b - a ) < tolerance ;
      }

      return true ;
    }

    public bool IsGreater(double b, double tolerance = Torlerance )
    {
      return a > b + tolerance ;
    }

    public bool IsGreaterEqual(double b, double tolerance = Torlerance )
    {
      if ( Math.Abs( b - a ) >= tolerance ) {
        return a > b + tolerance ;
      }

      return true ;
    }

    public bool IsGreaterThanOrEqual(double second, double tolerance = Torlerance)
    {
      return IsEqual(a, second, tolerance) || a > second;
    }

    public bool IsLessThanOrEqual(double second, double tolerance = Torlerance)
    {
      return IsEqual(a, second, tolerance) || a < second;
    }

    public bool IsEqual(double second, double tolerance = Torlerance)
    {
      var result = Math.Abs(a - second);
      return result <= tolerance;
    }
  }

  extension(double? first)
  {
    public bool IsLessThanOrEqual(double? second, double tolerance = Torlerance)
    {
      return first.IsEqualsTo( second, tolerance ) || first < second ;
    }

    public bool IsGreaterThanOrEqual(double? second, double tolerance = Torlerance)
    {
      return first.IsEqualsTo( second, tolerance ) || first > second ;
    }

    public bool IsGreaterThan(double? right, double tolerance = Torlerance )
    {
      if ( first != null && right != null )
        return IsGreater( first.Value, right.Value, tolerance ) ;
      return first == null && right == null ;
    }
  }

  extension(double? left)
  {
    public bool IsEqualsTo(double? right, double tolerance = Torlerance )
    {
      if ( left != null && right != null )
        return IsAlmostEqualsTo( left.Value, right.Value, tolerance ) ;
      return left == null && right == null ;
    }

    public bool IsSmallerThan(double? right, double tolerance = Torlerance )
    {
      if ( left != null && right != null )
        return IsSmaller( left.Value, right.Value, tolerance ) ;
      return left == null && right == null ;
    }
  }

  public static bool IsEqualsTo( this int? left, int? right )
  {
    if ( left != null && right != null )
      return left.Value.Equals( right.Value ) ;
    return left == null && right == null ;
  }
}