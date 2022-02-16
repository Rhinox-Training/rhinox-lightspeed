using System;

namespace Rhinox.Lightspeed
{
    public interface ICustomTypeResolver
    {
        bool CheckForType(string name, out Type foundType);
    }
}