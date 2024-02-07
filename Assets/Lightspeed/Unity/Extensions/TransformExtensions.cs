#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Lightspeed
{
    public static class TransformExtensions
    {
        public static void Reset(this Transform t, bool resetPosition = true, bool resetRotation = true, bool resetScale = true)
        {
            if (resetPosition)
                t.localPosition = Vector3.zero;
            if (resetRotation)
                t.localRotation = Quaternion.identity;
            if (resetScale)
                t.localScale = Vector3.one;
        }
        
        public static Quaternion TransformQuaternion(this Transform t, Quaternion quaternion)
        {
            return t.rotation * quaternion;
        }
        
        public static Quaternion InverseTransformQuaternion(this Transform t, Quaternion quaternion)
        {
            return Quaternion.Inverse(t.rotation) * quaternion;
        }
        
        public static Transform FindOrCreate(this Transform t, string n)
        {
            // if empty string, just return current transform
            if (string.IsNullOrWhiteSpace(n)) return t;

            // Attempt to find it by using a regular find
            var foundTransform = t.Find(n);

            if (foundTransform) return foundTransform;

            // If not found, start from the given transform and split the parts and create the needed parts
            return Create(t, n);
        }

        public static Transform Create(this Transform t, string n)
        {
            var parts = n.Split(',', ';', '/', '\\');

            foreach (var part in parts)
            {
                // if empty string, just return current transform
                if (string.IsNullOrWhiteSpace(part)) return t;

                // otherwise try to find it
                var newT = t.Find(part);
                if (newT != null) continue;

                // if not found, create it
                newT = new GameObject(part).transform;
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(newT.gameObject, "Create Transform");
#endif
                newT.SetParent(t);
                newT.Reset();

                t = newT;
            }

            return t;
        }

        public static void StripBehaviour<T>(this Transform t, bool includeChildren = false, bool destroyImmediate = false) where T : MonoBehaviour
        {
            var targets = includeChildren ? t.GetComponentsInChildren<T>() : t.GetComponents<T>();
            foreach (var target in targets)
            {
                if (destroyImmediate)
                    Object.DestroyImmediate(target);
                else
                    Object.Destroy(target);
            }
        }
        
        public static void CopyFrom(this Transform t, Transform other, bool copyPosition = true,
            bool copyRotation = true, bool copyScale = false, bool copyParent = false)
        {
            if (copyPosition)
                t.position = other.position;
            if (copyRotation)
                t.rotation = other.rotation;
            
            if (copyScale)
                t.localScale = other.localScale;

            if (copyParent)
                t.parent = other.parent;
        }

        public static void CopyLocalFrom(this Transform t, Transform other, bool copyPosition = true,
            bool copyRotation = true, bool copyScale = true, bool copyParent = false)
        {
            if (copyPosition)
                t.localPosition = other.localPosition;
            if (copyRotation)
                t.localRotation = other.localRotation;
            if (copyScale)
                t.localScale = other.localScale;
            if (copyParent)
                t.parent = other.parent;
        }
        
        public static IList<Transform> GetDirectChildren(this Transform obj)
        {
            var list = new List<Transform>();
            foreach (Transform child in obj)
            {
                list.Add(child);
            }

            return list;
        }
        
        public static IList<Transform> GetAllChildren(this Transform obj, bool includeInactive = false, bool includeSelf = true)
        {
            var transforms = obj.GetComponentsInChildren<Transform>(includeInactive);
            int length = includeSelf ? transforms.Length : transforms.Length - 1;
            var arr = new Transform[length];
             int idx = 0;

             foreach (var child in transforms)
             {
                 //if not include yourself, skip
                 if (!includeSelf && child == obj)
                     continue;
                 
                arr[idx] = child;
                ++idx;
             }
             
            return arr;
        }
        
        public static void GetAllChildren(this Transform obj, IList<Transform> transforms, bool includeInactive = false)
        {
            foreach (var t in obj.GetComponentsInChildren<Transform>(includeInactive))
                transforms.Add(t);
        }

        public static void DestroyAllChildren(this Transform obj)
        {
            var children = obj.GetAllChildren(false);
            foreach (var child in children)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        
        public static void SetPosition(this Transform t, Vector3 vec)
        {
            t.position = vec;
        }

        public static void SetPosition(this Transform t, float? x = null, float? y = null, float? z = null)
        {
            t.position = t.position.With(x, y, z);
        }

        public static void SetLocalPosition(this Transform t, Vector3 vec)
        {
            t.localPosition = vec;
        }

        public static void SetLocalPosition(this Transform t, float? x = null, float? y = null, float? z = null)
        {
            t.localPosition = t.localPosition.With(x, y, z);
        }
        
        public static void SetXPosition(this Transform transform, float xPos)
        {
            transform.position = transform.position.With(x: xPos);
        }

        public static void SetLocalXPosition(this Transform transform, float xPos)
        {
            transform.localPosition = transform.localPosition.With(x: xPos);
        }

        public static void SetYPosition(this Transform transform, float yPos)
        {
            transform.position = transform.position.With(y: yPos);
        }

        public static void SetLocalYPosition(this Transform transform, float yPos)
        {
            transform.localPosition = transform.localPosition.With(y: yPos);
        }

        public static void SetZPosition(this Transform transform, float zPos)
        {
            transform.position = transform.position.With(z: zPos);
        }

        public static void SetLocalZPosition(this Transform transform, float zPos)
        {
            transform.localPosition = transform.localPosition.With(z: zPos);
        }


        public static Vector3 DirectionTo(this Transform t, Transform other, bool normalized = true)
        {
            return t.position.DirectionTo(other.position, normalized);
        }

        public static Vector3 LocalDirectionTo(this Transform t, Transform other, bool normalized = true)
        {
            return t.localPosition.DirectionTo(other.localPosition, normalized);
        }
        
        public static float DistanceTo(this Transform t, Vector3 target)
        {
            return t.position.DistanceTo(target);
        }

        public static float DistanceTo(this Transform t, Transform other)
        {
            return t.position.DistanceTo(other.position);
        }

        public static float SqrDistanceTo(this Transform t, Vector3 target)
        {
            return t.position.SqrDistanceTo(target);
        }
        
        public static float SqrDistanceTo(this Transform t, Transform other)
        {
            return t.position.SqrDistanceTo(other.position);
        }

        public static void MoveTowards(this Transform t, Vector3 pos, float speed)
        {
            var newPos = Vector3.MoveTowards(t.position, pos, Time.deltaTime * speed);
            t.position = newPos;
        }
        
        public static void MoveTowardsSmooth(this Transform t, Vector3 pos, ref Vector3 velocity, float maxSpeed)
        {
            if (maxSpeed.LossyEquals(0f))
                return;
            
            var d = t.DistanceTo(pos);
            
            var deltaT = d / maxSpeed;
            
            var newPos = Vector3.SmoothDamp(t.position, pos, ref velocity, deltaT, maxSpeed, Time.deltaTime);
            t.position = newPos;
        }
        
        public static void RotateAround(this Transform t, Vector3 point, Quaternion rotation)
        {
            if (rotation == Quaternion.identity)
                return;
            Vector3 axis;
            float angle;
            rotation.ToAngleAxis(out angle, out axis);
            if (angle == 360.0f || angle == 0.0f)
                return;
            t.RotateAround(point, axis, angle);
        }
        
        public static void ShiftPivot(this Transform t, Vector3 localOffset)
        {
            var worldOffset = t.TransformDirection(localOffset);
            t.Translate(worldOffset, Space.World);
            foreach (Transform child in t)
                child.Translate(-worldOffset, Space.World);
        }
        
        public static void ShiftPivotTo(this Transform t, Vector3 newWorldPivot, bool recordUndo = false)
        {
#if UNITY_EDITOR
            if (recordUndo)
                Undo.RecordObject(t, "Center Pivot");
#endif
            var worldOffset  = newWorldPivot - t.position;
            
            t.position = newWorldPivot;
            foreach (Transform child in t)
            {
#if UNITY_EDITOR
                if (recordUndo)
                    Undo.RecordObject(child, "Center Pivot");
#endif
                child.Translate(-worldOffset, Space.World);
            }
        }

        public static Transform GroupTransforms(this Transform[] transforms, string groupName = null, bool recordUndo = false)
        {
            if (transforms == null || transforms.Length == 0)
                return null;
            
#if UNITY_EDITOR
            if (recordUndo)
                Undo.RegisterCompleteObjectUndo(transforms, "Group Object(s)");
#endif
            
            Vector3 pivotPosition = GetCenterPosition(transforms);

            if (string.IsNullOrWhiteSpace(groupName))
            {
                var names = transforms.Select(x => x.name).ToArray();
                var prefix = names.GetCommonPrefix();
                if (!string.IsNullOrWhiteSpace(prefix)) groupName = prefix;
            }
            
            GameObject groupObject = new GameObject(groupName ?? "New Group");
            groupObject.transform.position = pivotPosition;

            if (SameParent(transforms, out int siblingIndex, out Transform parent))
            {
                groupObject.transform.SetParent(parent);
                groupObject.transform.SetSiblingIndex(siblingIndex);
                
                transforms.Sort(SiblingIndexSorter);
            }


#if UNITY_EDITOR
            if (recordUndo)
                Undo.RegisterCreatedObjectUndo(groupObject, "Group Object(s)");
#endif
            
            foreach (Transform obj in transforms)
            {
#if UNITY_EDITOR
                if (recordUndo)
                    Undo.SetTransformParent(obj.transform, groupObject.transform, "Group Object(s)");
                else
                    obj.SetParent(groupObject.transform, true);
#else
                obj.SetParent(groupObject.transform, true);
#endif
            }
            
            return groupObject.transform;
        }

        private static int SiblingIndexSorter(Transform x, Transform y)
        {
            return x.GetSiblingIndex().CompareTo(y.GetSiblingIndex());
        }

        private static bool SameParent(Transform[] transforms, out int siblingIndex, out Transform parent)
        {
            siblingIndex = int.MaxValue;
            parent = null;
            
            for (var i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                if (parent != null && t.parent != parent)
                {
                    parent = null;
                    break;
                }

                siblingIndex = Mathf.Min(siblingIndex, t.GetSiblingIndex());
                parent = t.parent;
            }

            return parent != null;
        }

        public static Vector3 GetCenterPosition(this ICollection<Transform> transforms)
        {
            Vector3 position = Vector3.zero;
            if (transforms == null || transforms.Count == 0)
                return position;

            foreach (var transform in transforms)
                position += transform.position;
            return position / transforms.Count;
        }
        
        public static void ShiftScaleTo(this Transform t, Vector3 newScale, bool recordUndo = false)
        {
#if UNITY_EDITOR
            if (recordUndo)
                Undo.RecordObject(t, "Shift Scale");
#endif
            var worldScale = newScale.DivideBy(t.localScale);

            t.localScale = newScale;
            foreach (Transform child in t)
            {
#if UNITY_EDITOR
                if (recordUndo)
                    Undo.RecordObject(child, "Shift Scale");
#endif
                child.localPosition = child.localPosition.DivideBy(worldScale);
                child.localScale = child.localScale.DivideBy(worldScale);
            }
        }
        
        /// <summary>
        /// Does the action for each child of the given parent. The action has 1 param [GameObject] which is the child.
        /// </summary>
        public static void ForeachChild(this Transform parent, Action<GameObject> execute)
        {
            // create a new list so things like destroy will still be okay to call from the action
            var childeren = new List<GameObject>();
            for (int i = 0; i < parent.childCount; i++)
                childeren.Add(parent.GetChild(i).gameObject);

            foreach (var child in childeren)
                execute(child);
        }

        /// <summary>
        /// Does the action for each child of the given parent. The action has 2 param [GameObject] which is the child and an [int] which is the index of the child.
        /// </summary>
        public static void ForAllChilderen(this Transform parent, Action<GameObject, int> execute)
        {
            // create a new list so things like destroy will still be okey to call from the action
            var childeren = new List<GameObject>();
            for (int i = 0; i < parent.childCount; i++)
                childeren.Add(parent.GetChild(i).gameObject);

            for (int i = 0; i < childeren.Count; ++i)
                execute(childeren[i], i);
        }

        /// <summary>
        /// Converts RectTransform.rect's local coordinates to world space
        /// </summary>
        /// <returns>The world rect.</returns>
        public static Rect GetWorldRect(this RectTransform rt)
        {
            // Convert the rectangle to world corners and grab the top left
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var x = corners[0].DistanceTo(corners[3]);
            var y = corners[1].DistanceTo(corners[2]);
            return new Rect(corners[1], new Vector2(x, y));
        }
        
        /// <summary>
        /// Returns RectTransform.rect's but scale will be taken into account.
        /// </summary>
        /// <returns>The rect scaled to proportion.</returns>
        public static Rect GetScaledRect(this RectTransform rt)
        {
            var r = rt.rect;
            var wp = rt.transform.TransformPoint(r.position);
            var wpEnd =  rt.transform.TransformPoint(r.position + r.size);
            
            var dir = wp.DirectionTo(wpEnd, false);
            var size = rt.InverseTransformDirection(dir);

            var sizeD = new Vector2(r.size.x - size.x, r.size.y - size.y);

            return new Rect( r.position + (sizeD * .5f), size);
        }

        public static void ApplyRotationDifference(this Transform t, Quaternion difference, bool global = true)
        {
            if (global)
                t.rotation = t.rotation.ApplyDifference(difference);
            else
                t.localRotation = t.localRotation.ApplyDifference(difference);
        }
        
        
        public static string GetGameObjectPath(this Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        public static Transform AddChild(this Transform t, string name = null)
        {
            if (t == null)
                return null;

            GameObject go = new GameObject(name);
            go.transform.parent = t;
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        public static Vector3 GetAxisWorld(this Transform t, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return t.right;
                case Axis.Y:
                    return t.up;
                case Axis.Z:
                    return t.forward;
                default:
                    throw new ArgumentException("Axis is wrong range, only X,Y,Z are supported", nameof(axis));
            }
        }

        public static Pose GetWorldPose(this Transform t)
        {
            return new Pose(t.position, t.rotation);
        }
    }
}