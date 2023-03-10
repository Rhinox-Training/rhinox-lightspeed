using System;
using UnityEngine;

/// <summary>
/// Add this attribute to a class that requires a certain version of unity to function.
/// The given version is the minimum version required.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class UnitySupportWarningAttribute : PropertyAttribute
{
    public int Major { get; }
    public int Minor { get; }
    
    public Version Version { get; }
    public string VersionString { get; }

    public UnitySupportWarningAttribute(int major, int minor = 0)
    {
        Major = major;
        Minor = minor;
        Version = new Version(major, minor);
        VersionString = $"{Major}.{Minor}";
    }
}
