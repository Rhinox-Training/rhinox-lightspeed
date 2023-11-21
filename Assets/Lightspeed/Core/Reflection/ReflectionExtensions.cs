using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Rhinox.Lightspeed.Reflection
{
    public static partial class ReflectionExtensions
    {
        public static BindingFlags Without(this BindingFlags flags, BindingFlags other)
        {
            // AND NOT, same as XOR (^)
            return flags & ~other;
        }
        
        // TODO: add flatten?
        public static MethodInfo[] GetMethodsWithAttribute<T>(this Type t, bool publicMethods = true, bool instanceMethods = true)
            where T : Attribute
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
            return GetDefinedTypesOfType(domain, typeof(T), includeGeneric);
        }
        
        public static IEnumerable<Type> GetDefinedTypesOfType(this AppDomain domain, Type baseType, bool includeGeneric = false)
        {
            if (baseType == null)
                yield break;
            
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypesSafe())
                {
                    if (type == null)
                        continue;
                    
                    if (!type.IsClass || type.IsAbstract || (!includeGeneric && type.ContainsGenericParameters))
                        continue;
                    
                    if (!type.InheritsFrom(baseType))
                        continue;
                    yield return type;
                }
            }
        }
        
        public static IEnumerable<Type> GetDefinedTypesWithAttribute<T>(this AppDomain domain) where T : Attribute
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypesSafe())
                {
                    if (type == null)
                        continue;
                    
                    if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                        continue;
                    var attr = type.GetCustomAttribute<T>();
                    if (attr == null)
                        continue;
                    yield return type;
                }
            }
        }
        
        public static IEnumerable<Type> GetDefinedTypesWithAttribute(this AppDomain domain, Type baseAttributeType)
        {
            if (baseAttributeType == null || !typeof(Attribute).IsAssignableFrom(baseAttributeType))
                yield break;
            
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypesSafe())
                {
                    if (type == null)
                        continue;
                        
                    if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                        continue;
                    var attr = type.GetCustomAttribute(baseAttributeType);
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
        
        public static IEnumerable<EventInfo> GetEventsWithAttribute(this Type t, Type attributeType, bool publicMethods = true,
            bool instanceMethods = true)
        {
            if (attributeType == null || !typeof(Attribute).IsAssignableFrom(attributeType))
                yield break;
            
            BindingFlags access = publicMethods ? BindingFlags.Public : BindingFlags.NonPublic;
            BindingFlags instance = instanceMethods ? BindingFlags.Instance : BindingFlags.Static;

            var eventInfos = t.GetEvents(access | instance);
            foreach (EventInfo evt in eventInfos)
            {
                var javaScriptAttr = evt.GetCustomAttribute(attributeType);
                if (javaScriptAttr == null)
                    continue;

                yield return evt;
            }
        }

        public static bool TryGetAttribute<T>(this MemberInfo info, out T attribute)
            where T : Attribute
        {
            attribute = info.GetCustomAttribute<T>();
            return attribute != null;
        }
        
        public static bool TryGetAttribute<T>(this Type t, out T attribute)
            where T : Attribute
        {
            attribute = t.GetCustomAttribute<T>();
            return attribute != null;
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
        
        public static string GetNiceName(this ParameterInfo info, bool splitCamelCase = true)
        {
            string titleCase = info.Name.ToTitleCase();
            if (splitCamelCase)
                return titleCase.SplitCamelCase();
            return titleCase;
        }

        public static string GetNiceName(this MemberInfo info, bool splitCamelCase = true)
        {
            string titleCase = info.Name.ToTitleCase();
            if (splitCamelCase)
                return titleCase.SplitCamelCase();
            return titleCase;
        }

        public static string GetNiceName(this Type type, bool splitCamelCase = true)
        {
            if (!type.IsGenericTypeDefinition && type.IsGenericType)
            {
                string argStr = string.Join(", ", type.GetGenericArguments().Select(x => x.GetNiceName()));
                var regex = new Regex(@"\`([0-9]+)");
                string newName = regex.Replace(type.Name, $"<{argStr}>");
                string titleCaseReplaced = newName.ToTitleCase();
                if (splitCamelCase)
                    return titleCaseReplaced.SplitCamelCase();
                return titleCaseReplaced;
            }
            
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean: return "bool";
                case TypeCode.Byte: return "byte";
                case TypeCode.Char: return "char";
                case TypeCode.Int16: return "short";
                case TypeCode.Int32: return "int";
                case TypeCode.Int64: return "long";
                case TypeCode.Single: return "float";
                case TypeCode.Double: return "double";
                case TypeCode.String: return "string";
                case TypeCode.Decimal: return "decimal";
            }

            if (type == typeof(void)) return "void";
            
            string titleCase = type.Name.ToTitleCase();
            if (splitCamelCase)
                return titleCase.SplitCamelCase();
            return titleCase;
        }
        
        public static string GetFullNiceName(this Type type, bool splitCamelCase = true)
        {
            string separator = splitCamelCase ? "/" : ".";
            return type.Namespace + separator + GetNiceName(type, splitCamelCase);
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

        // NOTE: obscure blog post from investigation in CLR, this is the only way to check if a class is static
        // https://stackoverflow.com/a/2639465
        public static bool IsStatic(this Type t)
        {
            if (t == null)
                return false;
            return t.IsClass && t.IsSealed && t.IsAbstract;
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
        
        public static object GetValue(this MemberInfo memberInfo, object obj, params object[] parameters)
        {
            if (memberInfo == null) return null;
            
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(obj);
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(obj);
                case MethodInfo methodInfo:
                    if (methodInfo.GetCustomAttribute<ExtensionAttribute>() != null)
                        return methodInfo.Invoke(null, Utility.JoinArrays(obj, parameters));
                    return methodInfo.Invoke(obj, parameters);
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public static bool TrySetValue(this MemberInfo memberInfo, object obj, object val)
        {
            if (memberInfo == null) return false;
            
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(obj, val);
                    return true;
                case PropertyInfo propertyInfo:
                    if (propertyInfo.SetMethod == null)
                        return false;
                    propertyInfo.SetValue(obj, val);
                    return true;
                case MethodInfo methodInfo:
                    methodInfo.Invoke(obj, new[] {val});
                    return true;
                default:
                    return false;
            }
        }
        
        public static void SetValue(this MemberInfo memberInfo, object obj, object val)
        {
            if (memberInfo == null) return;
            
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(obj, val);
                    break;
                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue(obj, val);
                    break;
                case MethodInfo methodInfo:
                    methodInfo.Invoke(obj, new[] {val});
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public static bool HasParameters(this MethodInfo methodInfo, IList<Type> paramTypes, bool inherit = true)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length != paramTypes.Count)
                return false;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (!inherit)
                {
                    if (parameters[i].ParameterType != paramTypes[i])
                        return false;
                }
                else
                {
                    if (!paramTypes[i].InheritsFrom(parameters[i].ParameterType))
                        return false;
                }
            }

            return true;
        }
        
        public static bool IsDefined<T>(this ICustomAttributeProvider member) where T : Attribute => member.IsDefined<T>(false);
        
        public static bool IsDefined<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute
        {
            try
            {
                return member.IsDefined(typeof (T), inherit);
            }
            catch
            {
                return false;
            }
        }

        public static ICollection<Type> GetTypesSafe(this Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();
                return types;
            }
            catch (ReflectionTypeLoadException typeloadException)
            {
                return typeloadException.Types; // NOTE: ILGeneration may register null entries in this array
            }
        }
    }
}