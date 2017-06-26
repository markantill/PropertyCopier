using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using PropertyCopier.Comparers;
using PropertyCopier.Data;

namespace PropertyCopier
{
    /// <summary>
    /// Helper methods for figuring out type related stuff.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Checks if the type implements the generic interface.
        /// </summary>
        /// <param name="generic">The generic type.</param>
        /// <param name="toCheck">Target check type.</param>
        /// <returns>The type of the first generic argument if the Target check type implements the generic interface, otherwise null.</returns>
        internal static Type ImplementsGenericInterface(this Type toCheck, Type generic)
        {
            var interfaces = toCheck.GetInterfaces();
            if (toCheck.IsInterface)
            {
                interfaces = interfaces.Concat(new[] { toCheck }).ToArray();
            }
            var result =
                interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == generic);
            if (result != null)
            {
                result = result.GetGenericArguments().FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// Checks if the object can be null accounting for <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if type can be null, false otherwise.</returns>
        internal static bool CanBeNull(this Type type)
        {
            return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Gets the base classes and interfaces it implements.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type)
        {
            return type.BaseType == typeof(object)
                ? type.GetInterfaces()
                : Enumerable
                    .Repeat(type.BaseType, 1)
                    .Concat(type.GetInterfaces())
                    .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                    .Distinct();
        }

        /// <summary>
        /// Determines whether the given type is assignable to the specified generic type.
        /// </summary>
        /// <param name="givenType">Type of the given.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <returns></returns>
        internal static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
            {
                return false;
            }

            return givenType == genericType
              || givenType.MapsToGenericTypeDefinition(genericType)
              || givenType.HasInterfaceThatMapsToGenericTypeDefinition(genericType)
              || givenType.BaseType.IsAssignableToGenericType(genericType);
        }

        /// <summary>
        /// Determines whether the given type has an interface that maps to generic type definition.
        /// </summary>
        /// <param name="givenType">Type of the given.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <returns></returns>
        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return givenType
              .GetInterfaces()
              .Where(it => it.IsGenericType)
              .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        /// <summary>
        /// Mapses to generic type definition.
        /// </summary>
        /// <param name="givenType">Type of the given.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <returns></returns>
        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return genericType.IsGenericTypeDefinition
              && givenType.IsGenericType
              && givenType.GetGenericTypeDefinition() == genericType;
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        internal static ICollection<Tuple<PropertyInfo, bool>> GetAllProperties(
            Type type, 
            string properties, 
            ICollection<Tuple<PropertyInfo, bool>> results = null)
        {
            results = results ?? new List<Tuple<PropertyInfo, bool>>();

            var split = properties.Split('.');            

            var propertyName = split[0];               
            var propertyInfo = type.GetProperty(propertyName);

            var underlyingType = propertyInfo.PropertyType.ImplementsGenericInterface(typeof(IEnumerable<>));
            var isEnumerable = !(underlyingType == null || underlyingType == typeof(char));

            results.Add(new Tuple<PropertyInfo, bool>(propertyInfo, isEnumerable));

            if (split.Length == 1)
            {
                // Do this so we can use tail recursion for performance.
                return results;
            }

            var nextProperties = String.Join(".", split.Skip(1));
            var nextType = !isEnumerable ? propertyInfo.PropertyType : underlyingType;

            return GetAllProperties(nextType, nextProperties, results);
        }

        /// <summary>
        /// Gets the last property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>Type of penultimate property, name of last property.</returns>
        internal static Tuple<Type, string> GetLastProperty(Type type, string properties)
        {
            var split = properties.Split('.');
            if (split.Length == 1)
            {
                return new Tuple<Type, string>(type, properties);
            }

            var property = type.GetProperty(split[0]);
            var newProperties = String.Join(".", split.Skip(1));
            var underlyingType = GetIEnumerableGenericType(property);
            var newType = underlyingType == null || underlyingType == typeof(char)
                              ? property.PropertyType
                              : underlyingType;
            return GetLastProperty(newType, newProperties);
        }

        /// <summary>
        /// Gets the type of the i enumerable generic.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        internal static Type GetIEnumerableGenericType(PropertyInfo property)
        {
            var underlyingType = property.PropertyType.ImplementsGenericInterface(typeof(IEnumerable<>));
            return underlyingType;
        }

        /// <summary>
        /// Gets the generic method from the type matching the parameters.
        /// </summary>
        /// <param name="type">The type the method is on.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="genericTypeArgs">The generic type arguments.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="flags">The binding flags.</param>
        /// <returns>Generic method.</returns>
        internal static MethodBase GetGenericMethod(
            Type type,
            string name,
            Type[] genericTypeArgs,
            Type[] argTypes,
            BindingFlags flags)
        {
            int typeArity = genericTypeArgs.Length;
            var methods = type.GetMethods()
                .Where(m => m.Name == name)
                .Where(m => m.GetGenericArguments().Length == typeArity)
                .Where(m => m.GetParameters().Length == argTypes.Length)
                .Select(m => m.MakeGenericMethod(genericTypeArgs));

            return Type.DefaultBinder.SelectMethod(flags, methods.Cast<MethodBase>().ToArray(), argTypes, null);
        }

        /// <summary>
        /// Determines whether type is generic IEnumerable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if it is generic IEnumerable, false otherwise</returns>
        private static bool IsIEnumerable(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        /// <summary>
        /// Gets the generic IEnumerable implementation from a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type of the generic IEnumerable implementation.</returns>
        internal static Type GetIEnumerableImpl(Type type)
        {
            // Get IEnumerable implementation. Either type is IEnumerable<T> for some T, 
            // or it implements IEnumerable<T> for some T. We need to find the interface.
            if (IsIEnumerable(type))
                return type;
            var t = type.FindInterfaces((m, o) => IsIEnumerable(m), null);

            return t.FirstOrDefault();
        }

        /// <summary>
        /// The dictionary of types that can be cast.
        /// </summary>
        private static readonly Dictionary<Type, List<Type>> CastableValueTypes = new Dictionary<Type, List<Type>>
        {
            {
                typeof(decimal),
                new List<Type>
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(char)
                }
            },
            {
                typeof(double),
                new List<Type>
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(char),
                    typeof(float)
                }
            },
            {
                typeof(float),
                new List<Type>
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(char),
                    typeof(float)
                }
            },
            { typeof(ulong), new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(char) } },
            {
                typeof(long),
                new List<Type>
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(char)
                }
            },
            { typeof(uint), new List<Type> { typeof(byte), typeof(ushort), typeof(char) } },
            { typeof(int), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char) } },
            { typeof(ushort), new List<Type> { typeof(byte), typeof(char) } },
            { typeof(short), new List<Type> { typeof(byte) } }
        };

        /// <summary>
        /// Determines whether from type is castable to the to type.
        /// </summary>
        /// <param name="from">Source.</param>
        /// <param name="to">Target.</param>
        /// <returns></returns>
        internal static bool IsCastableTo(this Type from, Type to)
        {
            if (to.IsAssignableFrom(@from))
            {
                return true;
            }
            
            if (CastableValueTypes.ContainsKey(to) && CastableValueTypes[to].Contains(@from))
            {
                return true;
            }
            
            if(@from.IsEnum && IsCastableTo(to, typeof(int)))
            {
                return true;
            }

            if(to.IsEnum && IsCastableTo(@from, typeof(int)))
            {
                return true;
            }

            bool castable = @from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Any(
                                m => m.ReturnType == to &&
                                (m.Name == "op_Implicit" ||
                                m.Name == "op_Explicit")
                            );
            return castable;
        }

        /// <summary>
        /// Determines whether the specified type has a property with the specified name..
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal static bool HasProperty(this Type type, string name)
        {
            return type.GetProperties().Any(pi => pi.Name == name);
        }

        internal static IEnumerable<PropertyPair> GetNameMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? new DefaultStringComparer();

            var matches =
                sourceProperties
                .Where(s => s.CanRead)
                .Join(targetProperties.Where(t => t.CanWrite), 
                    s => s.Name, 
                    t => t.Name,
                    (s, t) => new PropertyPair { TargetProperty = t, SourceProperty = s }, comparer);
           
            return matches;
        }

        internal static void CheckTypesAreCompatable(
            PropertyInfo targetProperty,
            PropertyInfo sourceProperty)
        {
            // Check assignment from one property to another is possible.
            if (!sourceProperty.PropertyType.IsCastableTo(targetProperty.PropertyType))
            {
                throw new ArgumentException(
                    $"Property {sourceProperty.PropertyType.FullName} {sourceProperty.PropertyType.Name} {sourceProperty.Name} type cannot be mapped to: {targetProperty.PropertyType.FullName} {targetProperty.PropertyType.Name} {targetProperty.Name}");
            }
        }
    }
}
