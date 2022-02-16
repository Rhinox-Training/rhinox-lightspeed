using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using ReflectUtil = Rhinox.Lightspeed.ReflectionExtensions;

namespace Rhinox.Utilities
{
    public static class ReflectionExtensions
    {
        public static bool IsSerialized(this FieldInfo fieldInfo)
        {
            bool nonSerializedAttr = fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null;
            bool odinSerializeAttr = fieldInfo.GetCustomAttribute<OdinSerializeAttribute>() != null;
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
            bool odinSerializeAttr = propertyInfo.GetCustomAttribute<OdinSerializeAttribute>() != null;
            bool unitySerializeAttr = propertyInfo.GetCustomAttribute<SerializeField>() != null;
            
            if (nonSerializedAttr && !odinSerializeAttr)
                return false;

            if (!ReflectUtil.IsPublic(propertyInfo) && !(unitySerializeAttr || odinSerializeAttr))
                return false;

            return true;
        }

        public static bool IsSerialized(this MemberInfo info)
        {
            if (UnitySerializationUtility.GuessIfUnityWillSerialize(info))
                return true;
            
            if (UnitySerializationUtility.OdinWillSerialize(info, true))
                return true;

            return false;
        }
        
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
    }
}