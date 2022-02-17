using System;

namespace Rhinox.Lightspeed
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RefactoringOldNamespaceAttribute : Attribute
    {
        public string PreviousNamespace { get; }
        
        public RefactoringOldNamespaceAttribute(string namespaceString)
        {
            PreviousNamespace = namespaceString;
        }
    }
}