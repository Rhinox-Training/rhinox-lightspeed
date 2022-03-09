using System;
using Sirenix.OdinInspector;

namespace Rhinox.Lightspeed
{
    [EnumToggleButtons, Flags]
    public enum WindDirection
    {
        None = 0,
        
        N = 1 << 0,
        E = 1 << 1,
        W = 1 << 2,
        S = 1 << 3,
        
        NE = N | E,
        SE = S | E,
        SW = S | W,
        NW = N | W,
    }
}