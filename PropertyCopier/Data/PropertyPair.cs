using System;
using System.Reflection;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Represents a pair of properties that are mapped to each other.
    /// </summary>
    internal class PropertyPair
    {
        internal PropertyInfo TargetProperty { get; set; }

        internal PropertyInfo SourceProperty { get; set; }

        public override string ToString()
        {
            return $"{{ TargetProperty = {TargetProperty}, SourceProperty = {SourceProperty} }}";
        }
    }
}