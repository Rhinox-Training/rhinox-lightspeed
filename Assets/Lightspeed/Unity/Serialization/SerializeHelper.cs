using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Rhinox.Lightspeed.Reflection;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.Serialization;
#endif


namespace Rhinox.Lightspeed
{
    public static class SerializeHelper
    {
        public static IReadOnlyCollection<MemberInfo> GetPublicAndSerializedMembers<T>()
        {
            Type type = typeof(T);

            return GetPublicAndSerializedMembers(type);
        }

        public static IReadOnlyCollection<MemberInfo> GetPublicAndSerializedMembers(Type type)
        {
            var publicMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.GetField | BindingFlags.GetProperty |
                                                BindingFlags.FlattenHierarchy);
            publicMembers = publicMembers.Where(x => !(x is MethodBase)).ToArray();

            var serializedMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                                    BindingFlags.GetField | BindingFlags.GetProperty |
                                                    BindingFlags.FlattenHierarchy);
            serializedMembers = serializedMembers.Where(x => !(x is MethodBase) && x.IsSerialized()).ToArray();

            var list = new List<MemberInfo>();
            list.AddRange(publicMembers);
            list.AddRange(serializedMembers);

            return list.Distinct().ToArray();
        }


        public static IReadOnlyCollection<MemberInfo> GetSerializedMembers(Type type)
        {
            var members = new List<MemberInfo>();

            var fields = ReflectionUtility.GetAllFields(type);
            members.AddRange(fields);

#if ODIN_INSPECTOR
            var properties = ReflectionUtility.GetAllProperties(type);
            members.AddRange(properties.Where(x => x.GetMethod != null));
#endif

            var serializedMembers = members.Where(x => !(x is MethodBase) && x.IsSerialized()).ToArray();
            return serializedMembers;
        }

        public static bool IsSerialized(this FieldInfo fieldInfo)
        {
            bool nonSerializedAttr = fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null;
#if ODIN_INSPECTOR
            bool odinSerializeAttr = fieldInfo.GetCustomAttribute<OdinSerializeAttribute>() != null;
#else
            bool odinSerializeAttr = false;
#endif
            bool unitySerializeAttr = fieldInfo.GetCustomAttribute<SerializeField>() != null;
            if (nonSerializedAttr && !odinSerializeAttr)
                return false;

            if (!fieldInfo.IsPublic && !(unitySerializeAttr || odinSerializeAttr))
                return false;

            return true;
        }

        public static bool IsSerialized(this PropertyInfo propertyInfo)
        {
            bool nonSerializedAttr = propertyInfo.GetCustomAttribute<NonSerializedAttribute>() != null;
#if ODIN_INSPECTOR
            bool odinSerializeAttr = propertyInfo.GetCustomAttribute<OdinSerializeAttribute>() != null;
#else
            bool odinSerializeAttr = false;
#endif
            bool unitySerializeAttr = propertyInfo.GetCustomAttribute<SerializeField>() != null;

            if (nonSerializedAttr && !odinSerializeAttr)
                return false;

            if (!propertyInfo.IsPublic() && !(unitySerializeAttr || odinSerializeAttr))
                return false;

            return true;
        }

        public static bool IsSerialized(this MemberInfo info)
        {
            if (GuessIfUnityWillSerializePrivate(info))
                return true;

#if ODIN_INSPECTOR
            if (Sirenix.Serialization.UnitySerializationUtility.OdinWillSerialize(info, true))
                return true;
#endif
            return false;
        }

        private static bool GuessIfUnityWillSerializePrivate(MemberInfo member)
        {
            if (!(member is FieldInfo fieldInfo) ||
                fieldInfo.IsStatic ||
                fieldInfo.IsDefined<NonSerializedAttribute>())
                return false;
#if UNITY_2019_2_OR_NEWER
            if (fieldInfo.IsDefined<SerializeReference>(inherit: true))
                return true;
#endif

            if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType) &&
                fieldInfo.FieldType == fieldInfo.DeclaringType ||
                !fieldInfo.IsPublic &&
                !Rhinox.Lightspeed.Reflection.ReflectionExtensions.IsDefined<SerializeField>(fieldInfo))
                return false;
#if UNITY_2017_1_OR_NEWER
            if (fieldInfo.IsDefined<FixedBufferAttribute>())
                return true;
#endif

            return GuessIfUnityWillSerializePrivate(fieldInfo.FieldType);
        }

        private static readonly Type[] _unserializableUnityTypes = new[]
        {
            typeof(Coroutine)
        };

        private static bool GuessIfUnityWillSerializePrivate(System.Type type)
        {
            if (_unserializableUnityTypes.Contains(type))
                return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type) && type.GetGenericArguments().Length == 0)
                return true;
            if (type.IsAbstract || type.IsInterface || type == typeof(object))
                return false;
            if (type.IsEnum)
            {
                System.Type underlyingType = Enum.GetUnderlyingType(type);
                return underlyingType != typeof(long) && underlyingType != typeof(ulong);
            }

            bool isBaseDotNetType = type.IsPrimitive || type == typeof(string);
            if (isBaseDotNetType)
                return true;

            if (type.IsDelegateType())
                return false;

            if (typeof(UnityEventBase).IsAssignableFrom(type))
            {
#if !UNITY_2020_1_OR_NEWER
                if (type.IsGenericType)
                    return false;
#endif
                return type == typeof(UnityEvent) ||
                       type.IsDefined<SerializableAttribute>(inherit: false);
            }

            if (type.IsArray)
            {
                System.Type elementType = type.GetElementType();
                return type.GetArrayRank() == 1 &&
                       !elementType.IsArray &&
                       !elementType.ImplementsOpenGenericClass(typeof(List<>)) &&
                       GuessIfUnityWillSerializePrivate(elementType);
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition &&
                type.GetGenericTypeDefinition() == typeof(List<>))
            {
                System.Type type1 = type.GetArgumentsOfInheritedOpenGenericClass(typeof(List<>))[0];
                return !type1.IsArray &&
                       !type1.ImplementsOpenGenericClass(typeof(List<>)) &&
                       GuessIfUnityWillSerializePrivate(type1);
            }

            if (type.Assembly.FullName.StartsWith("UnityEngine", StringComparison.InvariantCulture) ||
                type.Assembly.FullName.StartsWith("UnityEditor", StringComparison.InvariantCulture))
                return true;

#if !UNITY_2020_1_OR_NEWER
            if (type.IsGenericType)
                return false;
#endif

            if (type.Assembly == typeof(string).Assembly) // No other .NET Types
                return false;

            if (type.IsDefined<SerializableAttribute>(inherit: false))
                return type.IsClass;

#if !UNITY_2018_2_OR_NEWER
                // Ported this hack from Sirenix.Serialization, ask them for more info
                for (System.Type baseType = type.BaseType;
                    baseType != null && baseType != typeof(object);
                    baseType = baseType.BaseType)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().FullName ==
                        "UnityEngine.Networking.SyncListStruct`1")
                        return true;
                }
#endif

            return false;
        }
    }
}