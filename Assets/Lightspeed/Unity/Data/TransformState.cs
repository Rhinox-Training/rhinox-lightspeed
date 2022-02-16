using System;
using UnityEngine;

namespace Rhinox.Utilities
{
    [Serializable]
    public struct TransformState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformState(Vector3 position, Quaternion rotation)
            : this(position, rotation, Vector3.one)
        {
        }

        public TransformState(Vector3 position)
            : this(position, Quaternion.identity, Vector3.one)
        {
        }

        public TransformState(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static TransformState Create(Transform t, bool world = true)
        {
            if (world)
                return new TransformState(t.position, t.rotation, t.lossyScale);
            return new TransformState(t.localPosition, t.localRotation, t.localScale);
        }
        
        public static TransformState CreateRelative(Transform t, Transform relativeTo)
        {
            var matrix = relativeTo.worldToLocalMatrix * t.localToWorldMatrix;
            return new TransformState(
                relativeTo.InverseTransformPoint(t.position),
                relativeTo.InverseTransformQuaternion(t.rotation),
                matrix.lossyScale
            );
        }
        
        public static void Restore(TransformState state, Transform t, bool world = true)
        {
            if (!world)
            {
                t.localPosition = state.Position;
                t.localRotation = state.Rotation;
                t.localScale = state.Scale;
                return;
            }
            
            t.position = state.Position;
            t.rotation = state.Rotation;
            if (t.parent == null)
                t.localScale = state.Scale;
            else
                t.localScale = state.Scale.DivideBy(t.parent.lossyScale);
        }
        
        public static void RestoreRelative(TransformState state, Transform t, Transform relativeTo)
        {
            t.position = relativeTo.TransformPoint(state.Position);
            t.rotation = relativeTo.TransformQuaternion(state.Rotation);
            
            var newScale = state.Scale.DivideBy(relativeTo.lossyScale);
            if (t.parent == relativeTo || t.parent == null)
                t.localScale = state.Scale;
            else
                t.localScale = newScale.DivideBy(t.parent.lossyScale);
        }
    }

    public static class TransformStateExtensions
    {
        public static void RestoreState(this Transform t, TransformState state, bool world = true)
        {
            TransformState.Restore(state, t, world);
        }
        
        public static void RestoreRelativeState(this Transform t, TransformState state, Transform relativeTo)
        {
            TransformState.RestoreRelative(state, t, relativeTo);
        }
    }
}