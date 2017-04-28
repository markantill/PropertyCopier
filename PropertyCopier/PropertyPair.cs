using System.Reflection;

namespace PropertyCopier
{
    internal class PropertyPair
    {
        internal PropertyInfo TargetProperty { get; set; }

        internal PropertyInfo SourceProperty { get; set; }

        public override string ToString()
        {
            return $"{{ TargetProperty = {TargetProperty}, SourceProperty = {SourceProperty} }}";
        }
    }

    internal class DefinedMappingPropertyPair : PropertyPair
    {
        public DefinedMapping DefinedMapping { get; set; }
    }
}