using System;

namespace Rhinox.Lightspeed.Reflection
{
    public interface ICustomTypeResolver
    {
        bool CheckForType(string name, out Type foundType);
    }
}