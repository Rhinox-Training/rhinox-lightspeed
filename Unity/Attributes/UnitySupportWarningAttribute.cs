using System;
using UnityEngine;

/// <summary>
/// Add this attribute to a class that requires a certain version of unity to function.
/// The given version is the minimum version required.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class UnitySupportWarningAttribute : PropertyAttribute
{
    public int Major;
    public int Minor;

    public UnitySupportWarningAttribute(int major, int minor = 0)
    {
        Major = major;
        Minor = minor;
    }
}
