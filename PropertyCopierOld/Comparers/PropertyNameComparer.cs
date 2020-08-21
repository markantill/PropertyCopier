using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyCopier.Comparers
{
    /// <summary>
    /// Used to see if two properties have the "same" name. Since different names
    /// can be mapped to each other we need to treat them as equal for comparison
    /// so the Linq query will work.
    /// </summary>
    public class PropertyNameComparer : IEqualityComparer<string>
    {
        private readonly StringComparer _comparer;
        private readonly IDictionary<string, string> _nameMatches;
        private readonly IDictionary<string, string> _reverseMatches;

        /// <summary>
        /// Initializes an instance of <see cref="PropertyNameComparer"/>.
        /// </summary>
        /// <param name="comparer">The <see cref="StringComparer"/> to use to compare the names.</param>
        public PropertyNameComparer(StringComparer comparer)
        {
            _comparer = comparer;
            _nameMatches = new Dictionary<string, string>(comparer);
            _reverseMatches = new Dictionary<string, string>(comparer);
        }

        /// <summary>
        /// Add a mapping of two string to treat as equivalent.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        public void AddMapping(string x, string y)
        {
            _nameMatches.Add(x, y);
            _reverseMatches.Add(y, x);
        }
        
        /// <summary>
        /// Check if the strings have been mapped, if so return true, 
        /// if not fall back to the <see cref="StringComparer"/> provided.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>true if the strings "match", false otherwise.</returns>
        public bool Equals(string x, string y)
        {
            string dictVal;
            if (_nameMatches.TryGetValue(x, out dictVal))
            {
                return _comparer.Equals(y, dictVal);
            }

            if (_reverseMatches.TryGetValue(x, out dictVal))
            {
                return _comparer.Equals(y, dictVal);
            }

            return _comparer.Equals(x, y);
        }

        /// <summary>
        /// Check if the string is a mapping, if so return the hashcode of the
        /// string that it is mapped to. Otherwise return the hashcode as calculated
        /// by the <see cref="StringComparer"/>.
        /// </summary>
        /// <param name="obj">The <see cref="string"/> to get the hashcode for.</param>
        /// <returns>The hashcode of the string it obj is mapped to, or if
        /// there isn't one the hashcode of the string.</returns>
        public int GetHashCode(string obj)
        {
            string dictVal;
            if (_nameMatches.TryGetValue(obj, out dictVal))
            {
                return _comparer.GetHashCode(dictVal);
            }

            return _comparer.GetHashCode(obj);
        }
    }
}