using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhinox.Lightspeed.Reflection
{
    public static class ReflectionUtility
    {
        private static IReadOnlyCollection<ICustomTypeResolver> _customTypeResolvers;

        public const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags ALL_FLAGS_WITH_INHERITED = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const MemberTypes ALL_MEMBERS = MemberTypes.Field | MemberTypes.Method | MemberTypes.Property;


        public static bool TryGetField(Type t, string name, out FieldInfo member, BindingFlags bindingAttr = ALL_FLAGS)
        {
            member = t.GetField(name, bindingAttr);
            bool memberFound = member != null;
            if (memberFound || !bindingAttr.HasFlag(BindingFlags.NonPublic))
                return memberFound;

            // Inherited privates are not returned (even with FlattenHierarchy), so search for it.
            return TryGetFieldRecursive(t.BaseType, name, out member, bindingAttr.Without(BindingFlags.FlattenHierarchy | BindingFlags.Public));
        }

        private static bool TryGetFieldRecursive(Type t, string name, out FieldInfo member, BindingFlags flags)
        {
            while (t != null)
            {
                member = t.GetField(name, flags);
                if (member != null)
                    return true;
                t = t.BaseType;
            }

            member = null;
            return false;
        }

        public static bool TryGetProperty(Type t, string name, out PropertyInfo member, BindingFlags bindingAttr = ALL_FLAGS_WITH_INHERITED)
        {
            member = t.GetProperty(name, bindingAttr);
            bool memberFound = member != null;
            if (memberFound || !bindingAttr.HasFlag(BindingFlags.NonPublic))
                return memberFound;

            // Inherited privates are not returned (even with FlattenHierarchy), so search for it.
            return TryGetPropertyRecursive(t.BaseType, name, out member, bindingAttr.Without(BindingFlags.FlattenHierarchy | BindingFlags.Public));
        }
        
        private static bool TryGetPropertyRecursive(Type t, string name, out PropertyInfo member, BindingFlags flags)
        {
            while (t != null)
            {
                member = t.GetProperty(name, flags);
                if (member != null)
                    return true;
                t = t.BaseType;
            }

            member = null;
            return false;
        }
        
        public static bool TryGetMethod(Type t, string name, out MethodInfo member, BindingFlags bindingAttr = ALL_FLAGS_WITH_INHERITED, bool includeExtensionMethods = false)
        {
            member = t.GetMethod(name, bindingAttr);
            if (includeExtensionMethods && member == null)
                member = t.GetExtensionMethod(name, bindingAttr);
            bool memberFound = member != null;
            if (memberFound || !bindingAttr.HasFlag(BindingFlags.NonPublic))
                return memberFound;

            // Inherited privates are not returned (even with FlattenHierarchy), so search for it.
            return TryGetMethodRecursive(t.BaseType, name, out member, bindingAttr.Without(BindingFlags.FlattenHierarchy | BindingFlags.Public), includeExtensionMethods);
        }
        
        public static bool TryGetMethodRecursive(Type t, string name, out MethodInfo member,
            BindingFlags flags = ALL_FLAGS_WITH_INHERITED, bool includeExtensionMethods = false)
        {
            while (t != null)
            {
                member = t.GetMethod(name, flags);
                if (includeExtensionMethods && member == null)
                    member = t.GetExtensionMethod(name, flags | BindingFlags.Public);
                if (member != null)
                    return true;
                t = t.BaseType;
            }

            member = null;
            return false;
        }

        public static bool TryGetMember(Type t, string name, out MemberInfo member, BindingFlags bindingAttr = ALL_FLAGS_WITH_INHERITED, bool includeExtensionMethods = false)
            => TryGetMember(t, ALL_MEMBERS, name, out member, bindingAttr, includeExtensionMethods);
        
        public static bool TryGetMember(Type t, MemberTypes allowedTypes, string name, out MemberInfo member, BindingFlags bindingAttr = ALL_FLAGS_WITH_INHERITED, bool includeExtensionMethods = false)
        {
            if (allowedTypes.HasFlag(MemberTypes.Field) && TryGetField(t, name, out FieldInfo fieldInfo, bindingAttr))
            {
                member = fieldInfo;
                return true;
            }

            if (allowedTypes.HasFlag(MemberTypes.Property) && TryGetProperty(t, name, out PropertyInfo propInfo, bindingAttr))
            {
                member = propInfo;
                return true;
            }
            
            if (allowedTypes.HasFlag(MemberTypes.Method) && TryGetMethod(t, name, out MethodInfo methodInfo, bindingAttr, includeExtensionMethods))
            {
                member = methodInfo;
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

        public static IEnumerable<FieldInfo> GetAllFields(Type type, Type lowestBase = null)
        {
            if (lowestBase == null)
                lowestBase = typeof(object);

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in type.GetFields(flags))
            {
                if (field.DeclaringType != type)
                    continue;
                yield return field;
            }

            type = type.BaseType;
            while (type != null && type != lowestBase)
            {
                foreach (var field in type.GetFields(flags))
                {
                    if (field.DeclaringType != type)
                        continue;
                    yield return field;
                }

                type = type.BaseType;
            }
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(Type type, Type lowestBase = null)
        {
            if (lowestBase == null)
                lowestBase = typeof(object);

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var propertyInfo in type.GetProperties(flags))
            {
                if (propertyInfo.DeclaringType != type)
                    continue;
                yield return propertyInfo;
            }

            type = type.BaseType;
            while (type != null && type != lowestBase)
            {
                foreach (var propertyInfo in type.GetProperties(flags))
                {
                    if (propertyInfo.DeclaringType != type)
                        continue;
                    yield return propertyInfo;
                }

                type = type.BaseType;
            }
        }
        
        

        public static IEnumerable<EventInfo> GetAllEvents(Type type, Type lowestBase = null)
        {
            if (lowestBase == null)
                lowestBase = typeof(object);

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var eventInfo in type.GetEvents(flags))
            {
                if (eventInfo.DeclaringType != type)
                    continue;
                yield return eventInfo;
            }

            type = type.BaseType;
            while (type != null && type != lowestBase)
            {
                foreach (var eventInfo in type.GetEvents(flags))
                {
                    if (eventInfo.DeclaringType != type)
                        continue;
                    yield return eventInfo;
                }

                type = type.BaseType;
            }
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
                FindTypes(ref list, types, predicate);
            }
            return list;
        }
        
        /// <summary>
        /// Fill the given list with all types that return true for the given delegate.
        /// </summary>
        private static void FindTypes(ref List<Type> outList, IList<Type> types, Func<Type, bool> predicate)
        {
            for (int i = 0; i < types.Count; ++i)
            {
                if (predicate(types[i]))
                    outList.Add(types[i]);
            }
        }

        /// <summary>
        /// <para>Gets a list of all usable types inheriting from the given type.</para>
        /// This does not include: Generics Definitions & Abstract classes
        /// </summary>
        public static List<Type> GetTypesInheritingFrom(Type t)
        {
#if UNITY_EDITOR
            var list = new List<Type>();
            var types = TypeCache.GetTypesDerivedFrom(t);
            FindTypes(ref list, types, x => IsBasicInheritedType(x, t));
            return list;
#else
            return GetTypesInheritingFrom(t, AppDomain.CurrentDomain.GetAssemblies());
#endif
        }
        
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
        
        public static Type FindTypeExtensively(ref string assemblyName, ref string typeName, bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(typeName))
                return null;

            string assemblyNameCopy = assemblyName;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == assemblyNameCopy);
            if (assembly != null)
            {
                var type = assembly.GetType(typeName, throwOnError);
                if (type != null) 
                    return type;
            }

            if (_customTypeResolvers == null)
                FindCustomTypeResolvers(ref _customTypeResolvers);
            
            // If type is not found, try the GetType overload with an assemblyResolver
            assembly = FindAssembly(assemblyName, typeName);
            if (assembly == null)
            {
                return null;
            }

            Type customResolvedType = DefaultTypeResolver(assembly, typeName, false);
            if (customResolvedType != null) // update the assemblyQualifiedName
            {
                typeName = customResolvedType.FullName;
                assemblyName = customResolvedType.Assembly.GetName().Name;
            }

            if (throwOnError && customResolvedType == null)
                throw new FileNotFoundException($"Could not find type for {typeName}, {assemblyName}");
            
            return customResolvedType;
        }
        
        public static Type FindTypeExtensively(ref string assemblyQualifiedName, bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
                return null;
            
            var type = Type.GetType(assemblyQualifiedName, false);
            if (type != null) 
                return type;

            var qualifiedName = new AssemblyQualifiedName(assemblyQualifiedName);
            qualifiedName.ValidateAndUpdate();

            type = Type.GetType(qualifiedName.ToString(), throwOnError);
            if (type != null) // update the assemblyQualifiedName
                assemblyQualifiedName = type.AssemblyQualifiedName;            
            
            return type;
        }

        private static void FindCustomTypeResolvers(ref IReadOnlyCollection<ICustomTypeResolver> customTypeResolvers)
        {
            var list = new List<ICustomTypeResolver>();
            foreach (var type in GetTypesInheritingFrom(typeof(ICustomTypeResolver)))
            {
                if (Activator.CreateInstance(type) is ICustomTypeResolver resolverInstance)
                    list.Add(resolverInstance);
            }

            customTypeResolvers = list;
        }

        private static Assembly FindAssembly(string assemblyName, string typeName)
        {
            if (_customTypeResolvers != null)
            {
                string searchTypeName = typeName;
                if (typeName.EndsWith("[]"))
                    searchTypeName = typeName.Substring(0, typeName.Length - 2);
                foreach (var assemblyResolver in _customTypeResolvers)
                {
                    if (assemblyResolver.CheckForAssembly(assemblyName, searchTypeName, out Assembly assembly))
                        return assembly;
                }
            }
            
            // Returns the assembly of the type by enumerating loaded assemblies in the app domain
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName == assemblyName)
                    return a;
            }
            return null;
        }

        private static Type DefaultTypeResolver(Assembly assembly, string typeName, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            var queryTypeName = typeName;
            bool isArrayType = typeName.EndsWith("[]");
            if (isArrayType)
                queryTypeName = queryTypeName.Substring(0, queryTypeName.Length - 2);
                
            
            // First check the given assembly; might just be in there
            if (TryFindTypeInAssembly(assembly, queryTypeName, comparison, out Type type))
                return isArrayType ? type.MakeArrayType() : type;

            if (_customTypeResolvers != null)
            {
                foreach (var typeResolver in _customTypeResolvers)
                {
                    if (typeResolver.CheckForType(queryTypeName, out Type customType))
                        return isArrayType ? customType.MakeArrayType() : customType;
                }
            }
            
            // Assumption: Type has moved assembly
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a == assembly) continue; // already handled this

                if (TryFindTypeInAssembly(a, queryTypeName, comparison, out type))
                    return isArrayType ? type.MakeArrayType() : type;
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
        
        /// <summary>
        /// Check whether MethodInfo is compatible with type.
        /// In the case of a MethodInfo with as DeclaringType a generic type, it will alter the MethodInfo to the implemented given type
        /// </summary>
        public static bool IsMethodOfType(Type type, ref MethodInfo mi)
        {
            if (type == mi.DeclaringType) 
                return true;
            
            // If we're not generic. just check assignable
            if (!mi.ContainsGenericParameters)
            {
                // TODO: InheritsFrom is more expensive but is it necessary here?
                return mi.DeclaringType.IsAssignableFrom(type);
            }

            // TypeCache can return methods from a generic class (i.e. <T>)
            // If our type is not implementing it, then it's definitely not compatible
            // ContainsGenericParameters is used to make sure it's our type that's generic
            // If the actual method is generic, nothing we can do about it
            if (!mi.DeclaringType.ContainsGenericParameters || !type.ImplementsOpenGenericClass(mi.DeclaringType))
                return false;
            
            // If so, try to get the typed version of this MethodInfo
            var implementedTypes = type.GetArgumentsOfInheritedOpenGenericClass(mi.DeclaringType);
            var typedBaseType = mi.DeclaringType.MakeGenericType(implementedTypes);
            return ReflectionUtility.TryGetMethod(typedBaseType, mi.Name, out mi);
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
            if (t.IsArray)
                return Array.CreateInstance(t.GetElementType(), 0);
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

        public static bool TryGetAncestryDistance(this Type parent, Type baseType, out int ancestry)
        {
            if (parent == null || baseType == null || !parent.InheritsFrom(baseType))
            {
                ancestry = -1;
                return false;
            }

            var current = parent;
            ancestry = 0;
            while (current != null)
            {
                if (current == baseType)
                    break;
                current = current.BaseType;
                ++ancestry;
            }

            return true;
        }

        public static int GetAncestryDistance(this Type parent, Type baseType)
        {
            TryGetAncestryDistance(parent, baseType, out int ancestry);
            return ancestry;
        }

        public static string SanitizeAssemblyQualifiedName(string text)
        {
            var qualifiedName = new AssemblyQualifiedName(text);
            qualifiedName.ValidateAndUpdate();
            return qualifiedName.ToString();
        }

        public static ICollection<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesWithAttribute<T>();
#else
            var list = new List<Type>();
            foreach (var potentialAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in potentialAssembly.GetTypes())
                {
                    if (type.HasCustomAttribute<RefactoringOldNamespaceAttribute>())
                        list.Add(type);
                }
            }
            return list;
#endif
        }
    }
}