using System;
using System.Linq;

namespace Rhinox.Lightspeed
{
    [RefactoringOldNamespace("Rhinox.Utilities")]
    public class RangeFloat
    {
        public float Start;
        public float End;

        public float Length => Math.Abs(End - Start);
        public float Center => (Start + End) / 2.0f;
        public RangeFloat(float start, float end)
        {
            if (end < start)
                throw new ArgumentException();
            
            Start = start;
            End = end;
        }
        
        public bool Contains(float val)
        {
            return val >= (Start - float.Epsilon) && val <= (End + float.Epsilon);
        }

        public bool Contains(params float[] vals)
        {
            return vals.All(val => val >= (Start - float.Epsilon) && val <= (End + float.Epsilon));
        }

        public bool Contains(RangeFloat otherRange)
        {
            return otherRange.Start >= (Start - float.Epsilon) && otherRange.End <= (End + float.Epsilon);
        }

        public void Clamp(float min, float max)
        {
            if (Start < min)
                Start = min;
            
            if (End > max)
                End = max;
        }

        public override string ToString()
        {
            return $"Range({Start:F} _ {End:F})";
        }
    }
}