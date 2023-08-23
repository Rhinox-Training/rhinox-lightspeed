using System;
using System.Reflection;

namespace Rhinox.Lightspeed
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
    public sealed class RefactoringOldNamespaceAttribute : Attribute
    {
        public string PreviousNamespace { get; }
        public string PreviousAssembly { get; }
        
        public RefactoringOldNamespaceAttribute(string namespaceString, string previousAssemblyString = null)
        {
            PreviousNamespace = namespaceString;
            PreviousAssembly = previousAssemblyString;
        }
    }
}