using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Rhinox.Utilities
{
#if ODIN_INSPECTOR
    [EnumToggleButtons]
#endif
    [Flags]
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