using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rhinox.Lightspeed.Reflection
{
    public class RefactorNamespacesTypeResolver : ICustomTypeResolver
    {
        private static Dictionary<string, Type> _refactoredTypesAttributes;
        
        public bool CheckForType(string typeName, out Type foundType)
        {
            if (_refactoredTypesAttributes == null)
            {
                _refactoredTypesAttributes = new Dictionary<string, Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.HasCustomAttribute<RefactoringOldNamespaceAttribute>())
                        {
                            var attr = type.GetCustomAttribute<RefactoringOldNamespaceAttribute>();
                            string oldName = $"{attr.PreviousNamespace}.{type.Name}";
                            string newName = type.FullName;
                            _refactoredTypesAttributes.Add(oldName, type);
                        }
                    }
                }
            }

            foundType = _refactoredTypesAttributes.GetOrDefault(typeName);
            return foundType != null;
        }
    }
}