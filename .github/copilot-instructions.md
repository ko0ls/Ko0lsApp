# Copilot Instructions

### Code Style'

- Use 2-space indentation for C#.
- Add space inside all parentheses and brackets: `if ( x == null )`, `foo( a, b )`, `arr[ i ]`.
- Same-line '{' for control flow (`if`/`for`/`while`), new-line `{` for types, methods, and lambdas.

```csharp
// OK
if ( x == null ) throw new ArgumentNullException( nameof( x ) );

// OK
if ( x == null ) {
  throw new ArgumentNullException( nameof( x ) );
}

// NG - multi-line without braces
if ( x == null )
  throw new ArgumentNullException( nameof( x ) );
```

## Unit Tests - Multiple Assertions

Use `Assert.EnterMultipleScope()` instead of `Assertion.Multiple( () => { ... })`.

- It is the newer NUnit API (introduced in NUnit 4) and avoids the nested lambda/closure overhead of `Assert.Multiple`.
- It keeps assertions at the same indentation level as the rest of test, which is more readable.

```csharp
// OK
using ( Assert.EnterMultipleScope() ) {
  Assert.That( result.Temperature, Is.EqualTo( expected ) );
  Assert.That( result.Humidity, Is.EqualTo( expected ) );
}

// NG
Assert.Multiple( () => {
  Assert.That( result.Temperature, Is.EqualTo( expected ) );
  Assert.That( result.Humidity, Is.EqualTo( expected ) );
} );
```