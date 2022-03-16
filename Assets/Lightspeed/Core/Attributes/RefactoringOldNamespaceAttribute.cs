using System;

namespace Rhinox.Lightspeed
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
    public sealed class RefactoringOldNamespaceAttribute : Attribute
    {
        public string PreviousNamespace { get; }
        
        public RefactoringOldNamespaceAttribute(string namespaceString)
        {
            PreviousNamespace = namespaceString;
        }
    }
}