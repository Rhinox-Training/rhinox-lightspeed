using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Rhinox.Lightspeed
{
    [EnumToggleButtons, Flags]
    public enum Axis
    {
        None = 0,
        
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        
        XYZ = X | Y | Z
    }
    
    public static class AxisExtensions
    {
        public static Vector3 GetAxis(this Transform t, Axis axis)
        {
            if (!axis.IsSingleFlag())
                throw new NotSupportedException("Multiple flags set on Axis.");

            if (axis.HasFlag(Axis.X)) return t.right;
            if (axis.HasFlag(Axis.Y)) return t.up;
            if (axis.HasFlag(Axis.Z)) return t.forward;
            
            throw new ArgumentException("Axis has no flags set.");
        }
        
        public static Vector3 ResetAxis(this Vector3 v, Axis lockAxis)
        {
            if (lockAxis.HasFlag(Axis.X)) v.x = 0;
            if (lockAxis.HasFlag(Axis.Y)) v.y = 0;
            if (lockAxis.HasFlag(Axis.Z)) v.z = 0;

            return v;
        }
        
        public static Vector2 StripValue(this Vector3 v, Axis axis)
        {
            if (axis.HasFlag(Axis.X)) return new Vector2(v.y, v.z);
            if (axis.HasFlag(Axis.Y)) return new Vector2(v.x, v.z);
            if (axis.HasFlag(Axis.Z)) return new Vector2(v.x, v.y);
            return v;
        }

        public static Vector3 FillValue(this Vector2 v, Axis axis, float fill = 0)
        {
            if (axis.HasFlag(Axis.X)) return new Vector3(fill, v.x, v.y);
            if (axis.HasFlag(Axis.Y)) return new Vector3(v.x, fill, v.y);
            if (axis.HasFlag(Axis.Z)) return new Vector3(v.x, v.y, fill);
            return v;
        }
        
        public static Vector3 FillValue(this Vector2 v, Axis axis, Vector3 fillVec)
        {
            if (axis.HasFlag(Axis.X)) return new Vector3(fillVec.x, v.x, v.y);
            if (axis.HasFlag(Axis.Y)) return new Vector3(v.x, fillVec.y, v.y);
            if (axis.HasFlag(Axis.Z)) return new Vector3(v.x, v.y, fillVec.z);
            return v;
        }


        /*public static Vector3 FillValues(this Axis axis, float fillValue, params float[] values)
        {
            if (values == null) return Vector3.zero;
            
            int i = 0;
            var v = new Vector3();
            
            if (values.Length-1 > i && axis.HasFlag(Axis.X)) v.x = values[i++];
            else v.x = fillValue;
            if (values.Length-1 > i && axis.HasFlag(Axis.Y)) v.y = values[i++];
            else v.y = fillValue;
            if (values.Length-1 > i && axis.HasFlag(Axis.Z)) v.z = values[i++];
            else v.z = fillValue;
            return v;
        }*/
        
        public static Vector3 ToVector(this Axis axis, float value)
        {
            if (axis.HasFlag(Axis.X)) return new Vector3(value, 0, 0);
            if (axis.HasFlag(Axis.Y)) return new Vector3(0, value, 0);
            if (axis.HasFlag(Axis.Z)) return new Vector3(0, 0, value);
            return Vector3.zero;
        }
        
        public static Quaternion ToQuaternion(this Axis axis, float value)
        {
            return Quaternion.Euler(axis.ToVector(value));
        }

    }
}