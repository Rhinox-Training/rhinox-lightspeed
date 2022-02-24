using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinox.Lightspeed.Reflection
{
    public static partial class ReflectionExtensions
    {
        public static bool ReturnsUnityObject(this MemberInfo memberInfo)
        {
            return memberInfo.GetReturnType().InheritsFrom(typeof(UnityEngine.Object));
        }
    }
}