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
    }
}