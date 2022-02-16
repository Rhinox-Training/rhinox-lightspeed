using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
using Sirenix.Serialization;
using Sirenix.Utilities;
#endif

namespace Rhinox.Lightspeed.Reflection
{
    public static partial class ReflectionExtensions
    {
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

            if (!IsPublic(propertyInfo) && !(unitySerializeAttr || odinSerializeAttr))
                return false;

            return true;
        }
        
#if ODIN_INSPECTOR
        public static bool IsSerialized(this MemberInfo info)
        {
            if (UnitySerializationUtility.GuessIfUnityWillSerialize(info))
                return true;
            
            if (UnitySerializationUtility.OdinWillSerialize(info, true))
                return true;

            return false;
        }
#endif
        
        public static bool ReturnsUnityObject(this MemberInfo memberInfo)
        {
            return memberInfo.GetReturnType().InheritsFrom(typeof(UnityEngine.Object));
        }
        
        public static IEnumerable<MemberInfo> GetNonUnityMemberOptions(this Type t)
        {
            return t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property)
                .Where(x => !x.GetReturnType().InheritsFrom(typeof(Object)));
        }

        // TODO: Test this method
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            if (!Attribute.IsDefined(type, typeof(T)))
                return default(T);
            return Attribute.GetCustomAttribute(type, typeof(T)) as T;
        }

        public static bool InheritsFrom(this Type t, Type otherType)
        {
            return otherType != null && otherType.IsAssignableFrom(t);
        }
    }
}