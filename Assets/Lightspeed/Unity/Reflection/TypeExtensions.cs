using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = System.Object;

namespace Rhinox.Lightspeed.Reflection
{
    public static class TypeExtensions
    {
        public static bool IsDelegateType(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }
        
        public static IEnumerable<MemberInfo> GetNonUnityMemberOptions(this Type t)
        {
            return t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property)
                .Where(x => !x.GetReturnType().InheritsFrom(typeof(Object)));
        }

        public static bool IsDefined<T>(this Type type, bool inherit) where T : Attribute
        {
            return type != null && type.IsDefined(typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            if (!HasCustomAttribute(type, typeof(T)))
                return default(T);
            return Attribute.GetCustomAttribute(type, typeof(T)) as T;
        }
        
        public static object GetCustomAttribute(this Type type, Type attributeType)
        {
            if (!HasCustomAttribute(type, attributeType))
                return null;
            return Attribute.GetCustomAttribute(type, attributeType);
        }

        public static Attribute[] GetCustomAttributes(this Type type)
        {
            if (type == null)
                return Array.Empty<Attribute>();
            return Attribute.GetCustomAttributes(type);
        }
        
        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            if (type == null)
                return Array.Empty<T>();
            return Attribute.GetCustomAttributes(type).OfType<T>().ToArray();
        }
        
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType)
        {
            if (type == null)
                return Array.Empty<Attribute>();
            var result = new List<Attribute>();
            var attributes = Attribute.GetCustomAttributes(type);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attribute = attributes[i];
                if (attributeType.IsInstanceOfType(attribute))
                    result.Add(attribute);
            }
            return result.ToArray();
        }

        public static bool HasCustomAttribute<T>(this Type type) where T : Attribute =>
            HasCustomAttribute(type, typeof(T));

        public static bool HasCustomAttribute(this Type type, Type attributeType)
        {
            return typeof(Attribute).IsAssignableFrom(attributeType) && Attribute.IsDefined(type, attributeType);
        }

        public static bool InheritsFrom(this Type t, Type otherType)
        {
            return otherType != null && otherType.IsAssignableFrom(t);
        }

        public static bool InheritsFrom<T>(this Type t)
        {
            return InheritsFrom(t, typeof(T));
        }
        
        public static bool ImplementsOpenGenericClass(this Type type, Type openGenericType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType)
                return true;
            Type baseType = type.BaseType;
            return baseType != null && baseType.ImplementsOpenGenericClass(openGenericType);
        }
        
        public static Type[] GetArgumentsOfInheritedOpenGenericClass(this Type type, Type openGenericType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType)
                return type.GetGenericArguments();
            Type baseType = type.BaseType;
            return baseType != null ? baseType.GetArgumentsOfInheritedOpenGenericClass(openGenericType) : Array.Empty<Type>();
        }
    }
}