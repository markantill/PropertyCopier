using System;
using System.Collections.Generic;

namespace PropertyCopier.Comparers
{
    internal class DefaultStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.ToUpperInvariant().GetHashCode();
        }
    }
}