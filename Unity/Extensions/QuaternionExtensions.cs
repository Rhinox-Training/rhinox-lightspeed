using System;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Utilities
{
    public static class QuaternionExtensions
    {
        public static bool LossyEquals(this Quaternion val1, Quaternion val2, float epsilon = float.Epsilon)
        {
            // Note: this assumes that these values will be Approx equal when they are almost the same TODO: is this correct?
            return val1.x.LossyEquals(val2.x, epsilon) &&
                   val1.y.LossyEquals(val2.y, epsilon) &&
                   val1.z.LossyEquals(val2.z, epsilon) &&
                   val1.w.LossyEquals(val2.w, epsilon);
        }
        
        public static Vector3 GetForward(this Quaternion quat)
        {
            return quat * Vector3.forward;
        }

        public static Vector3 GetUp(this Quaternion quat)
        {
            return quat * Vector3.up;
        }

        public static Vector3 GetRight(this Quaternion quat)
        {
            return quat * Vector3.right;
        }

        public static Vector3 GetAxis(this Quaternion quat, Axis axis)
        {
            if (!axis.IsSingleFlag())
                throw new NotSupportedException("Multiple flags set on Axis.");

            if (axis.HasFlag(Axis.X)) return quat.GetRight();
            if (axis.HasFlag(Axis.Y)) return quat.GetUp();
            if (axis.HasFlag(Axis.Z)) return quat.GetForward();
            
            throw new ArgumentException("Axis has no flags set.");
        }

        public static float AngleTo(this Quaternion q1, Quaternion q2)
        {
            var remainingRot = (q1 * Quaternion.Inverse(q2));

            float angle;
            Vector3 axis;
            remainingRot.ToAngleAxis(out angle, out axis);
            var remainingAngle = Mathf.Abs(angle > 180.0f ? angle - 360.0f : angle);
            return remainingAngle;
        }

        public static float Norm(this Quaternion q)
        {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            if (angle > 180.0f)
                angle -= 360.0f;
            return Mathf.Abs(angle);

        }
        
        public static Quaternion Difference(Quaternion from, Quaternion to)
        {
            // NOTE: Difference in pre and post multiplication in Unity
            // to = from * (Inverse(from) * to)
            // to = (to * Inverse(from)) * from <---- We use premultiplication since Unity uses this internally as well
            // Both are the same,  but should be consistently applied using the method ApplyDifference
            return Quaternion.Inverse(from) * to;
        }
        
        public static Quaternion ApplyDifference(this Quaternion q, Quaternion difference)
        {
            // Pre-multiply, see comment in method 'Difference'
            return q * difference;
        }

        public static string Print(this Quaternion _quaternion)
        {
            return ("(" + _quaternion.x.ToString("0.0#######") + ", " + _quaternion.y.ToString("0.0#######")
                    + ", " + _quaternion.z.ToString("0.0#######") + ", " + _quaternion.w.ToString("0.0#######") + ")");
        }
    }
}
