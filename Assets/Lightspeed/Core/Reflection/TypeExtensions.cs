using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
        
        public static Type GetCollectionElementType(this Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            Type baseGenericType = typeof(IEnumerable<>);
            Type[] genericImplementations;
        
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == baseGenericType)
                genericImplementations = collectionType.GetGenericArguments();
            else
                genericImplementations = collectionType.GetArgumentsOfInheritedOpenGenericClass(baseGenericType);

            if (genericImplementations.Length > 0)
                return genericImplementations[0];

            throw new ArgumentException("Not a collection type", nameof(collectionType));
        }
        
        public static string GetCSharpName(this Type type, Stack<Type> genericArgs = null, StringBuilder arrayBrackets = null, bool includeNameSpace = true)
        {
            StringBuilder code = new StringBuilder();
            Type declaringType = type.DeclaringType;

            bool arrayBracketsWasNull = arrayBrackets == null;

            if (genericArgs == null)
            {
                if (type.IsGenericTypeDefinition)
                    genericArgs = new Stack<Type>();
                else
                    genericArgs = new Stack<Type>(type.GetGenericArguments());
            }


            int currentTypeGenericArgsCount = genericArgs.Count;
            if (declaringType != null)
                currentTypeGenericArgsCount -= declaringType.GetGenericArguments().Length;

            Type[] currentTypeGenericArgs = new Type[currentTypeGenericArgsCount];
            for (int i = currentTypeGenericArgsCount - 1; i >= 0; i--)
                currentTypeGenericArgs[i] = genericArgs.Pop();


            if (declaringType != null)
                code.Append(GetCSharpName(declaringType, genericArgs)).Append('.');


            if (type.IsArray)
            {
                if (arrayBrackets == null)
                    arrayBrackets = new StringBuilder();

                arrayBrackets.Append('[');
                arrayBrackets.Append(',', type.GetArrayRank() - 1);
                arrayBrackets.Append(']');

                Type elementType = type.GetElementType();
                code.Insert(0, GetCSharpName(elementType, arrayBrackets: arrayBrackets));
            }
            else
            {
                code.Append(new string(type.Name.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').ToArray()));

                if (currentTypeGenericArgsCount > 0)
                {
                    code.Append('<');
                    for (int i = 0; i < currentTypeGenericArgsCount; i++)
                    {
                        code.Append(GetCSharpName(currentTypeGenericArgs[i]));
                        if (i < currentTypeGenericArgsCount - 1)
                            code.Append(',');
                    }

                    code.Append('>');
                }

                if (includeNameSpace && declaringType == null && !string.IsNullOrEmpty(type.Namespace))
                {
                    code.Insert(0, '.').Insert(0, type.Namespace);
                }
            }


            if (arrayBracketsWasNull && arrayBrackets != null)
                code.Append(arrayBrackets.ToString());


            return code.ToString();
        }
        
        public static IEnumerable<MethodInfo> GetExtensionMethodsNonAlloc(this Type t, BindingFlags bindingFlags = ReflectionUtility.ALL_FLAGS_WITH_INHERITED, bool simpleSearch = true)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || (!type.IsSealed && type.IsAbstract) || type.ContainsGenericParameters)
                        continue;

                    if (simpleSearch && (!type.IsStatic() || !type.Name.EndsWith("Extensions", StringComparison.InvariantCultureIgnoreCase)))
                        continue;
                    
                    foreach (var method in type.GetMethods(bindingFlags))
                    {
                        if (!method.IsStatic())
                            continue;
                        
                        if (method.GetCustomAttribute<ExtensionAttribute>() == null)
                            continue;
                        
                        var parameter = method.GetParameters()[0];
                        if (parameter.ParameterType != t)
                            continue;

                        yield return method;
                    }
                }
            }
        }

        public static ICollection<MethodInfo> GetExtensionMethods(this Type t, BindingFlags bindingFlags = ReflectionUtility.ALL_FLAGS_WITH_INHERITED, bool simpleSearch = true)
        {
            var methods = new List<MethodInfo>();
            foreach (var extensionMethod in GetExtensionMethodsNonAlloc(t, bindingFlags, simpleSearch))
                methods.Add(extensionMethod);
            return methods;
        }

        public static MethodInfo GetExtensionMethod(this Type t, string methodName, BindingFlags bindingFlags = ReflectionUtility.ALL_FLAGS_WITH_INHERITED, bool simpleSearch = true)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return null;
            
            foreach (var extensionMethod in GetExtensionMethodsNonAlloc(t, bindingFlags, simpleSearch))
            {
                if (extensionMethod.Name.CaseInvariantEquals(methodName))
                {
                    return extensionMethod;
                }
            }

            return null;
        }

        public static string GetShortAssemblyQualifiedName(this Type type)
        {
            return AssemblyQualifiedName.TrimVersion(type.AssemblyQualifiedName);
        }
    }
}