using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Rhinox.Lightspeed
{
    public static class ReflectionExtensions
    {
        public static MethodInfo[] GetMethodsWithAttribute<T>(this Type t, bool publicMethods = true,
            bool instanceMethods = true) where T : Attribute
        {
            BindingFlags access = publicMethods ? BindingFlags.Public : BindingFlags.NonPublic;
            BindingFlags instance = instanceMethods ? BindingFlags.Instance : BindingFlags.Static;

            return t.GetMethods(access| instance)
                .Where(x => x.GetCustomAttribute<T>() != null)
                .ToArray();
        }
        
        public static FieldInfo[] GetFieldsWithAttribute<T>(this Type t, bool publicMethods = true,
            bool instanceMethods = true) where T : Attribute
        {
            BindingFlags access = publicMethods ? BindingFlags.Public : BindingFlags.NonPublic;
            BindingFlags instance = instanceMethods ? BindingFlags.Instance : BindingFlags.Static;

            return t.GetFields(access| instance)
                .Where(x => x.GetCustomAttribute<T>() != null)
                .ToArray();
        }
        
        public static IEnumerable<Type> GetDefinedTypesOfType<T>(this AppDomain domain, bool includeGeneric = false)
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract || (!includeGeneric && type.ContainsGenericParameters))
                        continue;
                    
                    if (!typeof(T).IsAssignableFrom(type))
                        continue;
                    yield return type;
                }
            }
        }
        
        public static IEnumerable<Type> GetDefinedTypesWithAttribute<T>(this AppDomain domain) where T : Attribute
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                        continue;
                    var attr = type.GetCustomAttribute<T>();
                    if (attr == null)
                        continue;
                    yield return type;
                }
            }
        }

        public static IEnumerable<EventInfo> GetEventsWithAttribute<T>(this Type t, bool publicMethods = true,
            bool instanceMethods = true) where T : Attribute
        { 
            BindingFlags access = publicMethods ? BindingFlags.Public : BindingFlags.NonPublic;
            BindingFlags instance = instanceMethods ? BindingFlags.Instance : BindingFlags.Static;

            var eventInfos = t.GetEvents(access | instance);
            foreach (EventInfo evt in eventInfos)
            {
                var javaScriptAttr = evt.GetCustomAttribute<T>();
                if (javaScriptAttr == null)
                    continue;

                yield return evt;
            }
        }
        
        public static FieldInfo[] GetFieldOptions(this Type t)
        {
            return t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }
        
        public static PropertyInfo[] GetPropertyOptions(this Type t)
        {
            return t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, BindingFlags flags)
        {
            return type.GetMethods(flags)
                .Where(x => x.Name == name)
                .FirstOrDefault(x => x.IsGenericMethod);
        }
        
        public static bool IsStatic(this MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.IsStatic;
                case PropertyInfo propertyInfo:
                    return !propertyInfo.CanRead ? propertyInfo.GetSetMethod(true).IsStatic : propertyInfo.GetGetMethod(true).IsStatic;
                case MethodBase methodBase:
                    return methodBase.IsStatic;
                case EventInfo eventInfo:
                    return eventInfo.GetRaiseMethod(true).IsStatic;
                case Type type:
                    return type.IsSealed && type.IsAbstract;
                default:
                    throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to determine IsStatic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.", (object) member.DeclaringType.FullName, (object) member.Name, (object) member.GetType().FullName));
            }
        }
        
        public static bool IsPublic(this MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.IsPublic;
                case PropertyInfo propertyInfo:
                    return !propertyInfo.CanRead ? propertyInfo.GetSetMethod(true).IsPublic : propertyInfo.GetGetMethod(true).IsPublic;
                case MethodBase methodBase:
                    return methodBase.IsPublic;
                case EventInfo eventInfo:
                    return eventInfo.GetRaiseMethod(true).IsPublic;
                case Type type:
                    return type.IsPublic;
                default:
                    throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to determine IsPublic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.", (object) member.DeclaringType.FullName, (object) member.Name, (object) member.GetType().FullName));
            }
        }
        
        public static System.Type GetReturnType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case MethodInfo methodInfo:
                    return methodInfo.ReturnType;
                case EventInfo eventInfo:
                    return eventInfo.EventHandlerType;
                default:
                    return (System.Type) null;
            }
        }
    }
}