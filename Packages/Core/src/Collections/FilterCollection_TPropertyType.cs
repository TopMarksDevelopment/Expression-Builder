namespace TopMarksDevelopment.ExpressionBuilder.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterCollection<TPropertyType> : IFilterCollection<TPropertyType?>
{
    ICollection<TPropertyType?> _values { get; set; } = [];

    public FilterCollection(IEnumerable<TPropertyType?> values)
    {
        _values = values.ToList();
    }

    internal FilterCollection(IFilterCollection values)
    {
        var list = new List<TPropertyType?>().ToArray();

        values.CopyTo(list, 0);

        _values = [.. list];
    }

    public FilterCollection() { }

    public override string ToString()
    {
        var builder = new StringBuilder("[");

        foreach (var item in _values)
        {
            if (builder.Length > 1)
                builder.Append(", ");

            builder.Append(item?.ToString() ?? "null");
        }

        return builder.ToString() + "]";
    }

    public int Count => _values.Count;
    public bool Any() => _values.Any();

    public bool IsReadOnly => _values.IsReadOnly;

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    public void Add(TPropertyType? item) => _values.Add(item);

    public void Clear() => _values.Clear();

    public bool Contains(TPropertyType? item) => _values.Contains(item);

    public void CopyTo(TPropertyType?[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

    public bool Remove(TPropertyType? item) => _values.Remove(item);

    IEnumerator<TPropertyType?> IEnumerable<TPropertyType?>.GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    void ICollection.CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (array is not TPropertyType[] ppArray)
            throw new ArgumentException("Array not of correct type");

        CopyTo(ppArray, index);
    }

    public static explicit operator FilterCollection<TPropertyType>(List<TPropertyType> list)
        => new(list);
}
