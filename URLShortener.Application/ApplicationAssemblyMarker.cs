namespace URLShortener.Application;

/// <summary>
/// Marker type used to reference the <c>URLShortener.Application</c> assembly.
///
/// Some libraries (such as FluentValidation and AutoMapper) rely on
/// assembly scanning to automatically discover and register components like
/// validators, handlers, and mapping profiles. Instead of referencing a
/// specific implementation type, we expose this empty marker class so the
/// assembly can be safely and explicitly referenced when configuring
/// dependency injection.
///
/// Example usage:
/// <code>
/// services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
/// </code>
///
/// This avoids tight coupling to concrete classes and ensures the entire
/// Application layer can be scanned without modifying DI configuration when
/// new components are added.
/// </summary>
public sealed class ApplicationAssemblyMarker { }
