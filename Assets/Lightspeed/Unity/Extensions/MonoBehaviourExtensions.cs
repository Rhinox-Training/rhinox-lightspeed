using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rhinox.Lightspeed
{
	public static class MonoBehaviourExtensions
	{
		public static void GetFurthestFrom(this IEnumerable<Vector3> points, Vector3 position, out float sqrMaxDistance)
		{
			sqrMaxDistance = float.MinValue;

			if (points == null) return;

			foreach (var p in points)
			{
				var sqrDistance = (position - p).sqrMagnitude;

				if (sqrDistance < sqrMaxDistance)
					continue;

				sqrMaxDistance = sqrDistance;
			}
		}

		public static T GetFurthestFrom<T>(this IEnumerable<T> objects, Vector3 position, ref float sqrMaxDistance)
			where T : Component
		{
			if (objects == null) return null;

			T furthest = null;

			foreach (var o in objects)
			{
				var sqrDistance = (position - o.transform.position).sqrMagnitude;

				if (sqrDistance < sqrMaxDistance)
					continue;

				sqrMaxDistance = sqrDistance;
				furthest = o;
			}

			return furthest;
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, Vector3 position, T closest,
			ref float sqrSmallestDistance, Predicate<T> predicate = null)
			where T : Component
		{
			if (objects == null) return closest;

			foreach (var o in objects)
			{
				if (predicate != null && !predicate(o)) continue;

				var sqrDistance = (position - o.transform.position).sqrMagnitude;

				if (sqrDistance >= sqrSmallestDistance)
					continue;

				sqrSmallestDistance = sqrDistance;
				closest = o;
			}

			return closest;
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, T rootObj, Func<T, T, float> sortFunc,
			ref float smallestVal)
			where T : Component
		{
			T closest = null;
			if (objects == null) return closest;

			foreach (var o in objects)
			{
				var metric = sortFunc(rootObj, o);

				if (metric >= smallestVal)
					continue;

				smallestVal = metric;
				closest = o;
			}

			return closest;
		}

		public static void AlignParentTo<T>(this T child, Component target) where T : MonoBehaviour
		{
			var parent = child.transform.parent;

			parent.AlignWithBasedOnChild(child.transform, target.transform);
		}

		public static void AlignWith<T>(this Transform t, T target) where T : Component
		{
			t.position = target.transform.position;
			t.rotation = target.transform.rotation;
		}

		public static void AlignWithBasedOnChild<T>(this Transform parent, Transform child, T target)
			where T : MonoBehaviour
		{
			parent.AlignWithBasedOnChild(child, target.transform);
		}

		public static void AlignWithBasedOnChild(this Transform parent, Transform child, Transform target)
		{
			// parent.rotation = Quaternion.FromToRotation(v1, v2) * parent.rotation;
			parent.rotation = target.rotation * Quaternion.Inverse(child.transform.localRotation);
			parent.position = target.position + (parent.transform.position - child.transform.position);
		}

		public static void AlignWithBasedOnChild(this Transform parent, Vector3 worldOffset, Quaternion child,
			Transform target)
		{
			// parent.rotation = Quaternion.FromToRotation(v1, v2) * parent.rotation;
			parent.rotation = target.rotation * Quaternion.Inverse(child);
			parent.position = target.position + worldOffset;
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, Vector3 position, Predicate<T> predicate = null)
			where T : Component
		{
			var sqrSmallestDistance = float.MaxValue;

			return objects.GetClosestTo(position, null, ref sqrSmallestDistance, predicate);
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, MonoBehaviour other, Predicate<T> predicate = null)
			where T : Component
		{
			var d = float.MaxValue;

			return objects.GetClosestTo(other.transform.position, null, ref d, predicate);
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, MonoBehaviour other, ref float sqrSmallestDistance,
			Predicate<T> predicate = null)
			where T : Component
		{
			return objects.GetClosestTo(other.transform.position, null, ref sqrSmallestDistance, predicate);
		}

		public static T GetClosestTo<T>(this IEnumerable<T> objects, Component other, T closest,
			ref float sqrSmallestDistance, Predicate<T> predicate = null)
			where T : Component
		{
			return objects.GetClosestTo(other.transform.position, closest, ref sqrSmallestDistance, predicate);
		}

		public static float DistanceTo<T>(this T t, Component other) where T : Component
		{
			return t.transform.position.DistanceTo(other.transform.position);
		}

		public static float SqrDistanceTo<T>(this T t, Component other) where T : Component
		{
			return t.transform.position.SqrDistanceTo(other.transform.position);
		}

		public static T GetOrAddComponent<T>(this Component c, bool findInChildren = false) where T : Component
		{
			return c.gameObject.GetOrAddComponent<T>(findInChildren);
		}

		public static T GetOrAddComponent<T>(this Component c, out bool created, bool findInChildren = false) where T : Component
		{
			return c.gameObject.GetOrAddComponent<T>(out created, findInChildren);
		}

		public static T GetOrAddComponent<T>(this GameObject obj, bool findInChildren = false) where T : Component
		{
			return GetOrAddComponent<T>(obj, out _, findInChildren);
		}

		public static T GetOrAddComponent<T>(this GameObject obj, out bool created, bool findInChildren = false) where T : Component
		{
			
			var c = findInChildren ? obj.GetComponentInChildren<T>() : obj.GetComponent<T>();

			if (c == null)
			{
				created = true;
				c = obj.AddComponent<T>() as T;
			}
			else
				created = false;

			return c;
		}

		public static bool TryGetComponentInParent<T>(this Behaviour b, out T result)
		{
			result = b.GetComponentInParent<T>();
			return result != null;
		}

		public static void InvokeDelayed(this MonoBehaviour behaviour, Action action, float delay)
		{
			behaviour.StartCoroutine(InvokeDelayed(action, delay));
		}

		private static IEnumerator InvokeDelayed(Action action, float delay)
		{
			yield return new WaitForSeconds(delay);

			if (action != null)
				action.Invoke();
		}
		
		private const BindingFlags CopyFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

		public static bool CopyDataFrom(this Component comp, Component other)
		{
			PropertyInfo[] pinfos;
			FieldInfo[] finfos;
			
			Type type = comp.GetType();
			Type otherType = other.GetType();
			if (type == otherType || type.IsAssignableFrom(otherType))
			{
				pinfos = type.GetProperties(CopyFlags);
				finfos = type.GetFields(CopyFlags);
			}
			else if (otherType.IsAssignableFrom(type))
			{
				pinfos = otherType.GetProperties(CopyFlags);
				finfos = otherType.GetFields(CopyFlags);
			}
			else
				return false;
			
			foreach (var pinfo in pinfos)
			{
				if (!pinfo.CanWrite || !pinfo.CanRead)
					continue;
				
				if (pinfo.CustomAttributes.Any(x => x.AttributeType == typeof(ObsoleteAttribute)))
					continue;
				
				var value = pinfo.GetValue(other, null);
				pinfo.SetValue(comp, value, null);
			}

			foreach (var finfo in finfos)
			{
				if (finfo.CustomAttributes.Any(x => x.AttributeType == typeof(ObsoleteAttribute)))
					continue;
				
				var value = finfo.GetValue(other);
				finfo.SetValue(comp, value);
			}

			return true;
		}

		public static string GetFullName(this Component comp)
		{
			return comp.gameObject.GetFullName();
		}
	}
}