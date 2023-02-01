using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed.Reflection
{
    public static partial class TypeExtensions
    {
        public static string GetNameWithNesting(this Type type)
        {
            if (type.IsNested && !type.IsGenericParameter)
                return GetNameWithNesting(type.DeclaringType) + "." + type.Name;
            return type.Name;
        }

        public static bool IsDelegateType(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
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

        public static bool InheritsFrom(this Type t, Type possibleBaseType)
        {
            if (possibleBaseType == null)
                return false;
            
            if (t == possibleBaseType || possibleBaseType.IsAssignableFrom(t))
                return true;
            
            // Interface cannot implement a class
            if (t.IsInterface && !possibleBaseType.IsInterface)
                return false;
            
            if (possibleBaseType.IsInterface)
                return t.HasInterfaceType(possibleBaseType);
            
            // Handle generics
            if (possibleBaseType.IsGenericTypeDefinition)
            {
                for (Type baseType = t; baseType != null; baseType = baseType.BaseType)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == possibleBaseType)
                        return true;
                }
            }

            return false;
        }

        public static bool HasInterfaceType(this Type t, Type interfaceType)
        {
            if (interfaceType == null || !interfaceType.IsInterface)
                return false;
            
            return t.GetInterfaces().Contains(interfaceType);
        }
        
        public static bool HasInterfaceType<T>(this Type t)
        {
            return HasInterfaceType(t, typeof(T));
        }

        public static bool InheritsFrom<T>(this Type t)
        {
            return InheritsFrom(t, typeof(T));
        }

        public static bool ImplementsOpenGenericClass(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericType)
                return false;
                
            if (openGenericType.IsInterface)
            {
                var interfaceImplSet = type.GetInterfaces();
                if (interfaceImplSet.Length > 0)
                {
                    var matchingInterface = interfaceImplSet.FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == openGenericType);
                    if (matchingInterface != null)
                        return true;
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType)
                return true;
            Type baseType = type.BaseType;
            return baseType != null && baseType.ImplementsOpenGenericClass(openGenericType);
        }

        public static Type[] GetArgumentsOfInheritedOpenGenericClass(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericType)
                return Array.Empty<Type>();
                
            if (openGenericType.IsInterface)
            {
                var interfaceImplSet = type.GetInterfaces();
                if (interfaceImplSet.Length > 0)
                {
                    var matchingInterface = interfaceImplSet.FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == openGenericType);
                    if (matchingInterface != null)
                        return matchingInterface.GetGenericArguments();
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType)
                return type.GetGenericArguments();
            Type baseType = type.BaseType;
            return baseType != null
                ? baseType.GetArgumentsOfInheritedOpenGenericClass(openGenericType)
                : Array.Empty<Type>();
        }
    }
}