using System;
using System.Collections.Generic;

namespace Rhinox.Lightspeed
{
    public enum MemoryUnit
    {
        Bytes, KB, MB, GB, TB, PB, EB, ZB, YB
    }
    
    public readonly struct MemorySize
    {
        public readonly double Value;
        public readonly MemoryUnit Unit;

        public MemorySize(double value, MemoryUnit unit = MemoryUnit.Bytes)
        {
            Value = value;
            Unit = unit;
        }

        public double GetValue(MemoryUnit unit)
        {
            if (Unit == unit)
                return Value;
            
            var pow = (int)unit - (int)Unit;
            var newValue = Value / Math.Pow(1024, pow);
            return newValue;
        }
        
        public MemorySize(IEnumerable<MemorySize> sizes, MemoryUnit unit = MemoryUnit.Bytes)
        {
            Unit = unit;
            Value = 0;
            foreach (var size in sizes)
                Value += size.GetValue(unit);
        }

        public override string ToString() => $"{Value:F} {Unit}";
        public string ToString(MemoryUnit unit) => this.To(unit).ToString();

        public static MemorySize From(double value, MemoryUnit unit = MemoryUnit.Bytes)
        {
            return new MemorySize(value, unit);
        }

        public MemorySize To(MemoryUnit unit) => new MemorySize(GetValue(unit), unit);
    }
}