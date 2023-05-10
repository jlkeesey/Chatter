using System.Collections;
using System.Collections.Generic;

namespace System.Linq;

public class EnumerableWrapper<T> : IEnumerable<T>
{
    private readonly IEnumerator<T> _enumerator;

    public EnumerableWrapper(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}