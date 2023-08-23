using System;
using System.Reflection;

namespace Rhinox.Lightspeed.Reflection
{
    public interface ICustomTypeResolver
    {
        bool CheckForType(string name, out Type foundType);
        
        bool CheckForAssembly(string assemblyName, string typeName, out Assembly assembly);
    }
}