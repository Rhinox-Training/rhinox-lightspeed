using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rhinox.Lightspeed.Reflection
{
    public class RefactorNamespacesTypeResolver : ICustomTypeResolver
    {
        private static Dictionary<string, Type> _refactoredTypesAttributes;
        private Dictionary<string, Assembly> _refactoredAssemblies;
        
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

        public bool CheckForAssembly(string assemblyName, string typeName, out Assembly assembly)
        {
            if (_refactoredAssemblies == null)
            {
                _refactoredAssemblies = new Dictionary<string, Assembly>();
                foreach (var potentialAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in potentialAssembly.GetTypes())
                    {
                        if (type.HasCustomAttribute<RefactoringOldNamespaceAttribute>())
                        {
                            var attr = type.GetCustomAttribute<RefactoringOldNamespaceAttribute>();
                            if (string.IsNullOrWhiteSpace(attr.PreviousAssembly))
                                continue;

                            // if (assemblyName == attr.PreviousAssembly &&
                            //     typeName == GetTypeName(attr, type))
                            {
                                string key = $"{attr.PreviousAssembly}--{GetTypeName(attr, type)}";
                                if (_refactoredAssemblies.ContainsKey(attr.PreviousAssembly))
                                    continue;
                                _refactoredAssemblies.Add(key, potentialAssembly);
                            }
                        }
                    }
                }
            }

            assembly = _refactoredAssemblies.GetOrDefault($"{assemblyName}--{typeName}");
            return assembly != null;
        }

        private static string GetTypeName(RefactoringOldNamespaceAttribute attr, Type type)
        {
            if (!string.IsNullOrWhiteSpace(attr.PreviousNamespace))
                return $"{attr.PreviousNamespace}.{type.Name}";
            return type.Name;
        }
    }
}