using System.Reflection;

namespace PropertyCopier
{
    internal class PropertyPairChild
    {
        public PropertyInfo TargetProperty { get; set; }

        public PropertyInfo ChildProperty { get; set; }

        public PropertyInfo SourceProperty { get; set; }

        public override string ToString()
        {
            return $"{{ TargetProperty = {TargetProperty}, ChildProperty = {ChildProperty}, SourceProperty = {SourceProperty} }}";
        }
    }
}