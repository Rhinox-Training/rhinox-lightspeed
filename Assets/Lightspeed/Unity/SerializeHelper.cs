using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Utilities.Serialization
{
    public static class SerializeHelper
    {
        public static IReadOnlyCollection<MemberInfo> GetPublicAndSerializedMembers<T>(this T t)
        {
            Type type = typeof(T);

            var publicMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                            BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            publicMembers = publicMembers.Where(x => !(x is MethodInfo)).ToArray();
            
            var serializedMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                          BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            serializedMembers = serializedMembers.Where(x => !(x is MethodInfo) && x.IsSerialized()).ToArray();

            var list = new List<MemberInfo>();
            list.AddRange(publicMembers);
            list.AddRange(serializedMembers);

            return list.Distinct().ToArray();
        }
    }
}