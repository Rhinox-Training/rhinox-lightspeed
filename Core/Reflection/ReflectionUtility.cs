using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rhinox.Lightspeed.Reflection
{
    public static class ReflectionUtility
    {
        private static IReadOnlyCollection<ICustomTypeResolver> _customTypeResolvers;
        
        private const BindingFlags ALL_MEMBERS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        
        public static bool TryGetMember(Type t, string name, out MemberInfo member, BindingFlags bindingAttr = ALL_MEMBERS)
        {
            var fInfo = t.GetField(name, bindingAttr);
            if (fInfo != null)
            {
                member = fInfo;
                return true;
            }

            var pInfo = t.GetProperty(name, bindingAttr);
            if (pInfo != null)
            {
                member = pInfo;
                return true;
            }

            member = null;
            return false;
        }
        
        public static bool TryParseType(string str, out Type type)
        {
            try
            {
                type = Type.GetType(str);
                return type != null;
            }
            catch (Exception /*e*/)
            {
                type = null;
            }

            return false;
        }
        
        /// <summary>
        /// Gets a list of all types that return true for the given delegate.
        /// </summary>
        public static List<Type> FindTypes(Func<Type, bool> predicate)
            => FindTypes(predicate, AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        /// Gets a list of all types that return true for the given delegate.
        /// </summary>
        public static List<Type> FindTypes(Func<Type, bool> predicate, params Assembly[] assemblies)
        {
            List<Type> list = new List<Type>();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                var assembly = assemblies[i];
                var types = assembly.GetTypes();
                for (int j = 0; j < types.Length; ++j)
                {
                    if (predicate(types[j]))
                        list.Add(types[j]);
                }
            }
            return list;
        }

        /// <summary>
        /// <para>Gets a list of all usable types inheriting from the given type.</para>
        /// This does not include: Generics Definitions & Abstract classes
        /// </summary>
        public static List<Type> GetTypesInheritingFrom(Type t)
            => GetTypesInheritingFrom(t, AppDomain.CurrentDomain.GetAssemblies());
        
        /// <summary>
        /// <para>Gets a list of all usable types inheriting from the given type.</para>
        /// This does not include: Generics Definitions & Abstract classes
        /// </summary>
        public static List<Type> GetTypesInheritingFrom(Type t, params Assembly[] assemblies)
        {
            return FindTypes(x => IsBasicInheritedType(x, t), assemblies);
        }

        public static bool IsBasicInheritedType(Type type, Type baseType)
        {
            // We only want types we can create so Generic and Abstract are out
            if (type.IsAbstract || type.IsGenericTypeDefinition) return false;
            
            // Of course only when we can assign the type to its base is it valid    
            if (baseType.IsAssignableFrom(type)) return true;
            
            // There is 1 exception to this: if the baseType is generic
            if (!baseType.IsGenericType) return false;
            
            if (baseType.IsInterface)
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.GenericTypeArguments.Length > 0 && i.GetGenericTypeDefinition() == baseType)
                        return true;
                }
            }
            else
            {
                for (Type t = type; t != null; t = t.BaseType)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == baseType)
                        return true;
                }
            }
            return false;
        }
        
        public static Type[] GetGenericTypeImplementation(Type type, Type genericType)
        {
            if (!genericType.IsGenericType) return Array.Empty<Type>();

            if (genericType.IsInterface)
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                        return i.GenericTypeArguments;
                }
            }
            else
            {
                for (Type t = type; t != null; t = t.BaseType)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == genericType)
                        return t.GenericTypeArguments;
                }
            }

            return Array.Empty<Type>();
        }

        public static Type GetImplementedGenericType(Type type, Type genericType)
        {
            var implementedTypes = GetGenericTypeImplementation(type, genericType);
            return genericType.MakeGenericType(implementedTypes);
        }
        
        public static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   // type.IsValueType ||
                   type.EqualsOneOf(new Type[] { 
                       typeof(String),
                       typeof(Decimal),
                       typeof(DateTime),
                       typeof(DateTimeOffset),
                       typeof(TimeSpan),
                       typeof(Guid)
                   }) || Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool TryFindTypeInAssembly(Assembly assembly, string fullTypeName, StringComparison comparison, out Type type)
        {
            type = null;
            if (assembly == null) return false;
            
            foreach (var t in assembly.GetTypes())
            {
                if (string.IsNullOrWhiteSpace(t.FullName) || !t.FullName.Equals(fullTypeName, comparison))
                    continue;
                
                type = t;
                return true;
            }

            return false;
        }
        
        public static Type FindTypeExtensively(ref string assemblyQualifiedName, bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
                return null;
            
            var type = Type.GetType(assemblyQualifiedName);
            if (type != null) return type;

            if (_customTypeResolvers == null)
                FindCustomTypeResolvers(ref _customTypeResolvers);
            
            // If type is not found, try the GetType overload with an assemblyResolver
            type = Type.GetType(assemblyQualifiedName, DefaultAssemblyResolver, DefaultTypeResolver, throwOnError);

            if (type != null) // update the assemblyQualifiedName
                assemblyQualifiedName = type.AssemblyQualifiedName;

            return type;
        }

        private static void FindCustomTypeResolvers(ref IReadOnlyCollection<ICustomTypeResolver> customTypeResolvers)
        {
            var list = new List<ICustomTypeResolver>();
            foreach (var type in AppDomain.CurrentDomain.GetDefinedTypesOfType<ICustomTypeResolver>())
            {
                var resolverInstance = Activator.CreateInstance(type) as ICustomTypeResolver;
                if (resolverInstance != null)
                    list.Add(resolverInstance);
            }

            customTypeResolvers = list;
        }

        private static Assembly DefaultAssemblyResolver(AssemblyName assemblyName)
        {
            // Returns the assembly of the type by enumerating loaded assemblies in the app domain
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName == assemblyName.FullName)
                    return a;
            }
            return null;
        }

        private static Type DefaultTypeResolver(Assembly assembly, string name, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            // First check the given assembly; might just be in there
            if (TryFindTypeInAssembly(assembly, name, comparison, out Type type))
                return type;

            if (_customTypeResolvers != null)
            {
                foreach (var typeResolver in _customTypeResolvers)
                {
                    if (typeResolver.CheckForType(name, out Type customType))
                        return customType;
                }
            }
            
            // Assumption: Type has moved assembly
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a == assembly) continue; // already handled this

                if (TryFindTypeInAssembly(a, name, comparison, out type))
                    return type;
            }

            return null;
        }

        public static T FetchValuePropertyHelper<T>(Type memberDeclaringType, string methodSearchPath, object instance = null)
        {
            return (T) FetchValuePropertyHelper(typeof(T), memberDeclaringType, methodSearchPath, instance);
        }

        public static object FetchValuePropertyHelper(Type valueType, Type memberDeclaringType, string methodSearchPath, object instance = null)
        {
            if (string.IsNullOrWhiteSpace(methodSearchPath))
                throw new InvalidDataException("Cannot find ReferenceType for ValueLookup, MethodLookup is empty");

            string searchString = methodSearchPath;
            if (searchString.StartsWith("this.")) // instance
            {
                return GetMemberValue(valueType, searchString.Substring(5), memberDeclaringType, instance);
            }
            else if (searchString.Contains(".")) // static
            {
                int lastIndex = searchString.LastIndexOf('.');
                string typeString = searchString.Substring(0, lastIndex);
                string memberString = searchString.Substring(lastIndex);
                ReflectionUtility.TryParseType(typeString, out Type t);
                return GetMemberValue(valueType, memberString, t, instance);
            }
            else
            {
                return GetMemberValue(valueType, searchString, memberDeclaringType, instance);
            }
        }

        private static T GetMemberValue<T>(string memberString, Type ownerType, object instance)
        {
            return (T) GetMemberValue(typeof(T), memberString, ownerType, instance);
        }

        private static object GetMemberValue(Type memberType, string memberString, Type ownerType, object instance)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            
            var requestMembers = ownerType.GetMember(memberString, bindingFlags);
            requestMembers = requestMembers.Where(x => x.GetReturnType().IsAssignableFrom(memberType)).ToArray();
            foreach (var requestMember in requestMembers)
            {
                bool isStaticMember = requestMember.IsStatic();
                if (!isStaticMember && instance == null)
                    continue;
                var instanceRef = isStaticMember ? null : instance;
                
                if (requestMember is PropertyInfo propInfo)
                {
                    return propInfo.GetValue(instanceRef);
                }
                else if (requestMember is FieldInfo fieldInfo)
                {
                    return fieldInfo.GetValue(instanceRef);
                }
                else if (requestMember is MethodInfo methodInfo)
                {
                    if (methodInfo.GetParameters().Length == 0)
                        return methodInfo.Invoke(instanceRef, null);
                }
            }

            // Did not find
            return memberType.GetDefault();
        }
        
        public static object GetDefault(this Type t)
        {
            return typeof(ReflectionUtility).GetMethod(nameof(GetDefaultGeneric)).MakeGenericMethod(t).Invoke(null, null);
        }
        
        private static bool HasDefaultConstructor(this Type type)
        {
            foreach (var constr in type.GetConstructors())
            {
                if (constr.GetParameters().Length == 0)
                    return true;
            }
            return false;
        }
        
        public static T CreateInstance<T>()
        {
            return (T) CreateInstance(typeof(T));
        }

        public static object CreateInstance(this Type t)
        {
            if (t == null) return null;
            if (t.IsValueType || !t.HasDefaultConstructor())
                return t.GetDefault();
            return Activator.CreateInstance(t);
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public static bool SetField(this Type t, object instance, string name, object value)
        {
            var fieldInfo = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo == null)
                return false;

            fieldInfo.SetValue(instance, value);
            return true;
        }
        
        public static object GetFieldValue(this Type t, object instance, string name)
        {
            var fieldInfo = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo == null)
                return fieldInfo.FieldType.GetDefault();

            return fieldInfo.GetValue(instance);
        }
        
        public static T GetFieldValue<T>(this Type t, object instance, string name)
        {
            return (T) GetFieldValue(t, instance, name);
        }
        
        public static BindingFlags GetFlags(this MemberInfo info)
        {
            BindingFlags result = BindingFlags.Default;
            result |= (info.IsStatic() ? BindingFlags.Static : BindingFlags.Instance);
            result |= (info.IsPublic() ? BindingFlags.Public : BindingFlags.NonPublic);
            return result;
        }

        public static bool InvokeMethod(this Type t, object instance, string methodName, object[] args = null, bool includePrivate = false)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                flags |= BindingFlags.NonPublic;
            
            var methodInfo = t.GetMethod(methodName, flags);
            if (methodInfo == null)
                return false;

            methodInfo.Invoke(instance, args);
            return true;
        }
        
        public static Delegate CreateDelegate(MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType == (typeof(void));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction) {
                getType = Expression.GetActionType;
            }
            else {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }
        
        public static string MethodsOfObject(System.Object obj, bool includeInfo = false)
        {
            string methods = "";
            MethodInfo[] methodInfos = obj.GetType().GetMethods();
            for (int i = 0; i < methodInfos.Length; i++)
            {
                if (includeInfo)
                {
                    methods += methodInfos[i] + "\n";
                }

                else
                {
                    methods += methodInfos[i].Name + "\n";
                }
            }

            return (methods);
        }

        public static string MethodsOfType(System.Type type, bool includeInfo = false)
        {
            string methods = "";
            MethodInfo[] methodInfos = type.GetMethods();
            for (var i = 0; i < methodInfos.Length; i++)
            {
                if (includeInfo)
                {
                    methods += methodInfos[i] + "\n";
                }

                else
                {
                    methods += methodInfos[i].Name + "\n";
                }
            }

            return (methods);
        }
    }
}