using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoCADTools.Core.Localization;

namespace AutoCADTools.Presentation.Validation;

/// <summary>
/// Specifies that a field value must not be null.
/// Used as a safeguard for required fields (including double values
/// where WPF binding may throw FormatException before this fires).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CannotNullAttribute : ValidationAttribute
{
  public CannotNullAttribute()
  {
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    return value != null
      ? ValidationResult.Success
      : new ValidationResult(
        "Validation.CannotNull".GetString(),
        new List<string> { validationContext.MemberName! });
  }
}

/// <summary>
/// Specifies that a numeric field value must be greater than a given minimum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class GreaterThanAttribute : ValidationAttribute
{
  public double Minimum { get; }

  public GreaterThanAttribute(double minimum)
  {
    Minimum = minimum;
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value == null)
    {
      return new ValidationResult(
        "Validation.GreaterThan".GetString().Replace("{0}", Minimum.ToString()),
        new List<string> { validationContext.MemberName! });
    }

    bool success = value switch
    {
      double d => d > Minimum,
      int i => i > Minimum,
      float f => f > Minimum,
      long l => l > Minimum,
      _ => false
    };

    return success
      ? ValidationResult.Success
      : new ValidationResult(
        "Validation.GreaterThan".GetString().Replace("{0}", Minimum.ToString()),
        new List<string> { validationContext.MemberName! });
  }
}

/// <summary>
/// Specifies that a numeric field value must be greater than or equal to a given minimum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class GreaterThanOrEqualAttribute : ValidationAttribute
{
  public double Minimum { get; }

  public GreaterThanOrEqualAttribute(double minimum)
  {
    Minimum = minimum;
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value == null)
    {
      return new ValidationResult(
        "Validation.GreaterThanOrEqual".GetString().Replace("{0}", Minimum.ToString()),
        new List<string> { validationContext.MemberName! });
    }

    bool success = value switch
    {
      double d => d >= Minimum,
      int i => i >= Minimum,
      float f => f >= Minimum,
      long l => l >= Minimum,
      _ => false
    };

    return success
      ? ValidationResult.Success
      : new ValidationResult(
        "Validation.GreaterThanOrEqual".GetString().Replace("{0}", Minimum.ToString()),
        new List<string> { validationContext.MemberName! });
  }
}

/// <summary>
/// Specifies that a numeric field value must be smaller than a given maximum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class SmallerThanAttribute : ValidationAttribute
{
  public double Maximum { get; }

  public SmallerThanAttribute(double maximum)
  {
    Maximum = maximum;
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value == null)
    {
      return new ValidationResult(
        "Validation.SmallerThan".GetString().Replace("{0}", Maximum.ToString()),
        new List<string> { validationContext.MemberName! });
    }

    bool success = value switch
    {
      double d => d < Maximum,
      int i => i < Maximum,
      float f => f < Maximum,
      long l => l < Maximum,
      _ => false
    };

    return success
      ? ValidationResult.Success
      : new ValidationResult(
        "Validation.SmallerThan".GetString().Replace("{0}", Maximum.ToString()),
        new List<string> { validationContext.MemberName! });
  }
}

/// <summary>
/// Specifies that a numeric field value must be smaller than or equal to a given maximum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class SmallerThanOrEqualAttribute : ValidationAttribute
{
  public double Maximum { get; }

  public SmallerThanOrEqualAttribute(double maximum)
  {
    Maximum = maximum;
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value == null)
    {
      return new ValidationResult(
        "Validation.SmallerThanOrEqual".GetString().Replace("{0}", Maximum.ToString()),
        new List<string> { validationContext.MemberName! });
    }

    bool success = value switch
    {
      double d => d <= Maximum,
      int i => i <= Maximum,
      float f => f <= Maximum,
      long l => l <= Maximum,
      _ => false
    };

    return success
      ? ValidationResult.Success
      : new ValidationResult(
        "Validation.SmallerThanOrEqual".GetString().Replace("{0}", Maximum.ToString()),
        new List<string> { validationContext.MemberName! });
  }
}

/// <summary>
/// Specifies that a string field must not be null, empty, or whitespace.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class NotEmptyAttribute : ValidationAttribute
{
  public NotEmptyAttribute()
  {
  }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value is string s && !string.IsNullOrWhiteSpace(s))
    {
      return ValidationResult.Success;
    }
    return new ValidationResult(
      "Validation.NotEmpty".GetString(),
      new List<string> { validationContext.MemberName! });
  }
}
