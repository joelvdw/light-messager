/*
Copyright 2021, Joel VON DER WEID

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;

namespace LightMessager
{
  /// <summary>
  /// Thread-safe list using a global mutex
  /// </summary>
  /// <typeparam name="T">List items type</typeparam>
  class BlockingList<T> :ICollection<T>, IEnumerable<T>, IList<T>
  {
    private readonly List<T> internalList;
    private readonly object _lock = new object();

    public T this[int index]
    {
      get
      {
        T val = default(T);
        Exception ex = null;
        lock (_lock)
        {
          try
          {
            val = internalList[index];
          }
          catch (Exception e)
          {
            ex = e;
          }
        }
        if (ex != null)
          throw ex;
        return val;
      }
      set
      {
        Exception ex = null;
        lock (_lock)
        {
          try
          {
            internalList[index] = value;
          }
          catch (Exception e)
          {
            ex = e;
          }
        }
        if (ex != null)
          throw ex;
      }
    }

    public int Count => internalList.Count;

    public bool IsReadOnly => false;

    public BlockingList()
    {
      internalList = new List<T>();
    }
    public BlockingList(int capacity)
    {
      internalList = new List<T>(capacity);
    }

    public void Add(T item)
    {
      lock (_lock)
      {
        internalList.Add(item);
      }
    }

    public void Clear()
    {
      lock (_lock)
      {
        internalList.Clear();
      }
    }

    public bool Contains(T item)
    {
      bool c;
      lock (_lock)
      {
        c = internalList.Contains(item);
      }
      return c;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      Exception ex = null;
      lock (_lock)
      {
        try
        {
          internalList.CopyTo(array, arrayIndex);
        }
        catch (Exception e)
        {
          ex = e;
        }
      }
      if (ex != null)
        throw ex;
    }

    public IEnumerator<T> GetEnumerator() => Clone().GetEnumerator();

    public int IndexOf(T item)
    {
      int index;
      lock (_lock)
      {
        index = internalList.IndexOf(item);
      }
      return index;
    }

    public void Insert(int index, T item)
    {
      Exception ex = null;
      lock (_lock)
      {
        try
        {
          internalList.Insert(index, item);
        }
        catch (Exception e)
        {
          ex = e;
        }
      }
      if (ex != null)
        throw ex;
    }

    public bool Remove(T item)
    {
      bool c;
      lock (_lock)
      {
        c = internalList.Remove(item);
      }
      return c;
    }

    public void RemoveAt(int index)
    {
      Exception ex = null;
      lock (_lock)
      {
        try
        {
          internalList.RemoveAt(index);
        }
        catch (Exception e)
        {
          ex = e;
        }
      }
      if (ex != null)
        throw ex;
    }

    IEnumerator IEnumerable.GetEnumerator() => Clone().GetEnumerator();

    public List<T> Clone()
    {
      List<T> newList = new List<T>();

      lock (_lock)
      {
        internalList.ForEach(x => newList.Add(x));
      }

      return newList;
    }
  }
}
