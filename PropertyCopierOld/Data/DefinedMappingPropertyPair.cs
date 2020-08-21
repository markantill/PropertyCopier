using System;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Represents a pair of properties mapped to each other with a rule.
    /// </summary>
    internal class DefinedMappingPropertyPair : PropertyPair
    {
        public DefinedMapping DefinedMapping { get; set; }
    }
}