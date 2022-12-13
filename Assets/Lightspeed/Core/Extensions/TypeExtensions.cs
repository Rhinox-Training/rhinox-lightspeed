using System;

namespace Rhinox.Lightspeed
{
    public static class TypeExtensions
    {
        public static string GetNameWithNesting(this Type type)
        {
            if (type.IsNested && !type.IsGenericParameter)
                return GetNameWithNesting(type.DeclaringType) + "." + type.Name;
            return type.Name;
        }
    }
}