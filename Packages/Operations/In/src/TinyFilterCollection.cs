namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Collections;

/// <summary>
/// A tiny class to support ToString() handling like the FilterCollection in the Core package
/// Used when the underlying type is changed
/// </summary>
/// <typeparam name="TPropertyType">The type to be activated with</typeparam>
class TinyFilterCollection<TPropertyType> : ICollection<TPropertyType?>, IList
{
    readonly ICollection<TPropertyType?> _values = [];

    public TinyFilterCollection() { }

    public int Add(object? value)
    {
        if (value is TPropertyType t)
        {
            _values.Add(t);
            return _values.Count - 1;
        }

        throw new TypeLoadException();
    }

    public bool Contains(TPropertyType? item) => _values.Contains(item);

    public override string ToString() =>
        "["
        + string.Join(", ", _values.Select(x => x?.ToString() ?? "null"))
        + "]";

    public IEnumerator<TPropertyType?> GetEnumerator() =>
        _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public bool IsFixedSize => throw new NotImplementedException();

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    public object? this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void Add(TPropertyType? item) => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();

    public void CopyTo(TPropertyType?[] array, int arrayIndex) =>
        throw new NotImplementedException();

    public bool Remove(TPropertyType? item) =>
        throw new NotImplementedException();

    public bool Contains(object? value) => throw new NotImplementedException();

    public int IndexOf(object? value) => throw new NotImplementedException();

    public void Insert(int index, object? value) =>
        throw new NotImplementedException();

    public void Remove(object? value) => throw new NotImplementedException();

    public void RemoveAt(int index) => throw new NotImplementedException();

    public void CopyTo(Array array, int index) =>
        throw new NotImplementedException();
}
