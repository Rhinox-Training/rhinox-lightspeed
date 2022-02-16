using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Utilities
{
    public static partial class Utility
    {
        /// <summary>
        /// Clamps the given angle between 0 & 360
        /// </summary>
        public static float AngleClamp(float angle)
        {
            while (angle < 0)
                angle += 360f;

            return angle % 360f;
        }

        /// <summary>
        /// Checks if the angle lies between the given bounds
        /// </summary>
        public static bool AngleIsBetween(float min, float max, float angle)
        {
            min = AngleClamp(min);
            max = AngleClamp(max);

            if (min > max)
                return (angle >= max && angle <= min);

            return angle >= min && angle <= max;
        }
        
        public static float GetClosestAngle(float delta, Vector2 bounds, float step, bool flipped = false)
        {
            if (flipped)
                delta = (delta * -1.0f) + 180;

            if (delta < 0)
                delta = 360 + delta;

            var boundAngle = (delta + 180) % 360;
            if (boundAngle > bounds.y)
            {
                boundAngle = bounds.y;
            }
            else if (boundAngle < bounds.x)
            {
                boundAngle = bounds.x;
            }

            delta = (boundAngle + 180) % 360;

            if (step == -1)
                return delta;

            int steps = Mathf.RoundToInt(delta / step);
            return steps * step;
        }

        /// <summary>
        /// Brings an angle between -180 and 180
        /// </summary>
        public static float WrapAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Returns the approximate lerp value
        /// </summary>
        public static float InverseLerp(Vector3 start, Vector3 end, Vector3 value)
        {
            if (start.LossyEquals(value, .001f)) return 0;
            if (end.LossyEquals(value, .001f)) return 1;
            
            var lerps = new float[] {
                start.x.LossyEquals(end.x) ? 1 : Mathf.InverseLerp(start.x, end.x, value.x),
                start.y.LossyEquals(end.y) ? 1 : Mathf.InverseLerp(start.y, end.y, value.y),
                start.z.LossyEquals(end.z) ? 1 : Mathf.InverseLerp(start.z, end.z, value.z)
            };

            // If all values are equal, return that
            bool allEqual = true;
            for (var i = 1; i < lerps.Length; i++)
            {
                if (lerps[i].LossyEquals(lerps[0])) continue;
                
                allEqual = false;
                break;
            }

            if (allEqual) return lerps[0];
            
            // Return an average of all non-0 and non-1 values
            // IF it is 0 or 1, it should be caught by the previous loop
            return lerps
                .Where(x => x > 0 && x < 1) // Ignore 0 and 1 values; as they could be static values
                .Average();

        }
    }
}