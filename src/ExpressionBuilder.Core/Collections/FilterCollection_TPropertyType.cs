namespace TopMarksDevelopment.ExpressionBuilder.Collections;

using System.Collections;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterCollection<TPropertyType>
    : IFilterCollection<TPropertyType?>
{
    readonly ICollection<TPropertyType?> _values = [];

    public FilterCollection(IEnumerable<TPropertyType?> values)
    {
        _values = values.ToList();
    }

    internal FilterCollection(IFilterCollection values)
    {
        TPropertyType?[] list = [];

        values.CopyTo(list, 0);

        _values = [.. list];
    }

    public FilterCollection() { }

    public override string ToString() =>
        "["
        + string.Join(", ", _values.Select(x => x?.ToString() ?? "null"))
        + "]";

    public int Count => _values.Count;

    public bool Any() => _values.Any();

    public bool IsReadOnly => _values.IsReadOnly;

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    public void Add(TPropertyType? item) => _values.Add(item);

    public void AddRange(IEnumerable<TPropertyType?> items)
    {
        foreach (var item in items)
            _values.Add(item);
    }

    public void Clear() => _values.Clear();

    public bool Contains(TPropertyType? item) => _values.Contains(item);

    public void CopyTo(TPropertyType?[] array, int arrayIndex) =>
        _values.CopyTo(array, arrayIndex);

    public bool Remove(TPropertyType? item) => _values.Remove(item);

    IEnumerator<TPropertyType?> IEnumerable<TPropertyType?>.GetEnumerator() =>
        _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    void ICollection.CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (array is not TPropertyType[] ppArray)
            throw new ArgumentException("Array not of correct type");

        CopyTo(ppArray, index);
    }

    public void AddRange(IEnumerable items) =>
        AddRange(items.Cast<TPropertyType?>());

    public static explicit operator FilterCollection<TPropertyType>(
        List<TPropertyType> list
    ) => new(list);
}
