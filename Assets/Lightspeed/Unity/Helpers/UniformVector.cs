using System;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [Serializable]
    public struct UniformVector
    {
        public UniformVector(float value)
        {
            Value = value;
        } 
        
        public float Value;

        public static implicit operator Vector3(UniformVector vec) => new Vector3(vec.Value, vec.Value, vec.Value);
        public static implicit operator Vector2(UniformVector vec) => new Vector2(vec.Value, vec.Value);

        public static UniformVector operator +(UniformVector vec, float value) => new UniformVector(vec.Value + value);
        public static UniformVector operator -(UniformVector vec, float value) => new UniformVector(vec.Value - value);
        public static UniformVector operator *(UniformVector vec, float value) => new UniformVector(vec.Value * value);
        public static UniformVector operator /(UniformVector vec, float value) => new UniformVector(vec.Value / value);
        
        public static UniformVector operator +(float value, UniformVector vec) => new UniformVector(vec.Value + value);
        public static UniformVector operator -(float value, UniformVector vec) => new UniformVector(vec.Value - value);
        public static UniformVector operator *(float value, UniformVector vec) => new UniformVector(vec.Value * value);
        public static UniformVector operator /(float value, UniformVector vec) => new UniformVector(vec.Value / value);
    }
}