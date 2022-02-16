#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace Rhinox.Utilities
{
    // TODO: build custom drawer
    public struct Line
    {
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        public Vector3 Start { get; }
        
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
        public Vector3 End { get; }

        public Vector3 Direction => (End - Start).normalized;

        public Vector3 Center => (Start + End) / 2;
        
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideInEditorMode]
#endif
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