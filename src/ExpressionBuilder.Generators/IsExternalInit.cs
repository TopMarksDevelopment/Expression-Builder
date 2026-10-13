// netstandard2.0 predates init-only setters; the compiler only needs this type to exist.
namespace System.Runtime.CompilerServices;

using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
static class IsExternalInit { }
