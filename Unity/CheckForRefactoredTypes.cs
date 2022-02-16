#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;

namespace Rhinox.Lightspeed.Reflection
{
    public class CheckForRefactoredTypes : ICustomTypeResolver
    {
        private static Dictionary<string, Type> _refactoredTypesAttributes;
        
        public bool CheckForType(string typeName, out Type foundType)
        {
            if (_refactoredTypesAttributes == null)
            {
                var type = typeof(BindTypeNameToTypeAttribute);
                var oldNameField = type.GetField("OldTypeName", BindingFlags.Public | BindingFlags.Instance);
                var newTypeField = type.GetField("NewType", BindingFlags.Public | BindingFlags.Instance);

                if (oldNameField == null || newTypeField == null)
                {
                    _refactoredTypesAttributes = new Dictionary<string, Type>();
                    foundType = null;
                    return false;
                }

                _refactoredTypesAttributes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetCustomAttributes<BindTypeNameToTypeAttribute>())
                    .ToDictionary(
                        x => (string) oldNameField.GetValue(x),
                        x => (Type) newTypeField.GetValue(x));
            }

            foundType = _refactoredTypesAttributes.GetOrDefault(typeName);
            return foundType != null;
        }
    }
}
#endif