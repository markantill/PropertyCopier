using System;
using System.Reflection;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Represents a pair of properties where a child of one property is mapped to another.
    /// e.g. Foo.Bar -> FooBar
    /// </summary>
    internal class PropertyPairChild : PropertyPair
    {
        public PropertyInfo ChildProperty { get; set; }     

        public override string ToString()
        {
            return $"{{ TargetProperty = {TargetProperty}, ChildProperty = {ChildProperty}, SourceProperty = {SourceProperty} }}";
        }
    }
}