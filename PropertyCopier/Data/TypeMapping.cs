using System;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Used as a key to store the type mapping information.
    /// </summary>
    internal class TypeMapping : IEquatable<TypeMapping>
    {
        public TypeMapping(Type source, Type target)
        {
            Source = source;
            Target = target;
        }

        public Type Source { get; }

        public Type Target { get; }

        public bool Equals(TypeMapping other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;         
            return Equals(Source, other.Source) && Equals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeMapping) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Source?.GetHashCode() ?? 0) * 397 ^ (Target?.GetHashCode() ?? 0);
            }
        }
    }
}
