using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Extensions
{
    /// <summary>
    /// Helper methods for <see cref="MemberInfo"/>.
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Get the <see cref="Type"/> of the object returned by the memberInfo.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="Type"/> returned.</returns>
        public static Type GetReturnType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Constructor:
                    return memberInfo.DeclaringType;
                case MemberTypes.Event:
                    return ((EventInfo)memberInfo).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;                    
                case MemberTypes.Method:
                    return ((MethodInfo)memberInfo).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                case MemberTypes.TypeInfo:
                    return ((TypeInfo)memberInfo).DeclaringType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
