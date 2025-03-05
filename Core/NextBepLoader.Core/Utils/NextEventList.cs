using System;
using System.Collections;
using System.Collections.Generic;

namespace NextBepLoader.Core.Utils;

public class NextEventList<T>(Action<ListEventType, T?> onEvent) : IList<T>
{
    private List<T> BaseList { get; } = [];

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        onEvent.Invoke(ListEventType.Add, item);
        if (item != null)
            BaseList.Add(item);
    }

    public void CopyTo(T[] array, int arrayIndex) => BaseList.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        onEvent?.Invoke(ListEventType.Remove, item);
        return BaseList.Remove(item);
    }

    public int Count => BaseList.Count;

    public void Clear()
    {
        if (Count <= 0) return;
        onEvent?.Invoke(ListEventType.Clear, default);
        BaseList.Clear();
    }

    public bool Contains(T item)
    {
        onEvent.Invoke(ListEventType.Contains, item);
        return BaseList.Contains(item);
    }

    public IEnumerator<T> GetEnumerator() => BaseList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(T item) => BaseList.IndexOf(item);

    public void Insert(int index, T item) => BaseList.Insert(index, item);

    public void RemoveAt(int index) => BaseList.RemoveAt(index);

    public T this[int index]
    {
        get => BaseList[index];
        set => BaseList[index] = value;
    }
}

public enum ListEventType
{
    Add,
    Clear,
    Contains,
    Remove
}
