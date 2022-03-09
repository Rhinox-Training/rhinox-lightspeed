using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    // NOTE: Requires Odin for proper rendering
    // TODO: build custom drawer
    public struct Line
    {
        [ShowInInspector, ReadOnly, HideInEditorMode]
        public Vector3 Start { get; }
        
        [ShowInInspector, ReadOnly, HideInEditorMode]
        public Vector3 End { get; }

        public Vector3 Direction => (End - Start).normalized;

        public Vector3 Center => (Start + End) / 2;
        
        [ShowInInspector, ReadOnly, HideInEditorMode]
        public float Length { get; }

        public Line(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;

            Length = (start - end).magnitude;
        }

        public Line TransformToWorld(Transform t)
        {
            return new Line(
                t.TransformPoint(Start),
                t.TransformPoint(End)
            );
        }
        
        public Line TransformToLocal(Transform t)
        {
            return new Line(
                t.InverseTransformPoint(Start), 
                t.InverseTransformPoint(End)
            );
        }

        public Line Rotate(Quaternion rotation)
        {
            Vector3 direction = End - Start;

            Vector3 rotated = rotation * direction;
            
            return new Line(Start, Start + rotated);
        }
    }
}