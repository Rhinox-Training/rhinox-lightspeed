using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Rhinox.Lightspeed.Reflection
{
    public static partial class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetNonUnityMemberOptions(this Type t)
        {
            return t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property)
                .Where(x => !x.ReturnsUnityObject());
        }
    }
}