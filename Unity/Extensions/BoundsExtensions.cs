using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Utilities
{
    public static  class BoundsExtensions
    {
	    public static BoundingSphere Encapsulate(this BoundingSphere bounds, Vector3 pos, float rad = 0f)
	    {
		    var dir = bounds.position.DirectionTo(pos);
		    var dToPos = dir.magnitude + rad;
		    if (dToPos <= bounds.radius)
			    return bounds;
		    
		    var center = bounds.position + dir.normalized * ((dToPos - bounds.radius) / 2);
		    rad = (dToPos + bounds.radius) / 2;
		    return new BoundingSphere(center, rad);
	    }

	    public static bool Contains(this BoundingSphere bounds, Vector3 pos)
	    {
		    var sqDis = bounds.position.SqrDistanceTo(pos);
		    return sqDis <= Mathf.Pow(bounds.radius, 2);
	    }
	    
        public static float GetVolume(this Bounds bounds)
		{
			return bounds.size.x * bounds.size.y * bounds.size.z;
		}

		public static Vector3 ClosestPointOnSurface(this Bounds bounds, Vector3 p)
		{
			var collisionPoint = p;

			if (!bounds.Contains(p))
				return bounds.ClosestPoint(p);

			var extents = bounds.extents;
			var center = bounds.center;
			
			if (p.x > extents.x + center.x)
				collisionPoint.x = extents.x + center.x;
			else if (p.x < -extents.x + center.x)
				collisionPoint.x = -extents.x + center.x;

			if (p.y > extents.y + center.y)
				collisionPoint.y = extents.y + center.y;
			else if (p.y < -extents.y + center.y)
				collisionPoint.y = -extents.y + center.y;

			if (p.z > extents.z + center.z)
				collisionPoint.z = extents.z + center.z;
			else if (p.z < -extents.z + center.z)
				collisionPoint.z = -extents.z + center.z;

			return collisionPoint;
		} 
		
		public static Vector3[] GetCorners(this Bounds bounds)
		{
			return new[]
			{
				bounds.min,
				new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
				
				new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
				new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
				bounds.max,
			};
		}

		public static Bounds GetObjectBounds(this GameObject go, Renderer[] renderers = null, Collider[] colliders = null)
		{
			if (colliders == null) colliders = go.GetComponentsInChildren<Collider>();
			if (colliders.Any())
			{
				var bounds = colliders.GetCombinedBounds();
				return bounds;
			}
			if (renderers == null) renderers = go.GetComponentsInChildren<Renderer>();
			if (renderers.Any())
			{
				var bounds = renderers.GetCombinedBounds();
				return bounds;
			}

			return default;
		}

		public static Bounds GetCombinedBounds(this IEnumerable<Collider> colliders)
		{
			return colliders.Select(x => x.bounds).ToArray().Combine();
		}
		
		public static Bounds GetCombinedBounds(this ICollection<Collider> colliders)
		{
			if (colliders.Count == 0) return default(Bounds);
			
			var b = colliders.ElementAt(0).bounds;
			
			for (int i = 1; i < colliders.Count; ++i)
				b.Encapsulate(colliders.ElementAt(i).bounds);
			return b;
		}

		public static float GetVolume(this IEnumerable<Collider> colliders)
		{
			return colliders.GetCombinedBounds().GetVolume();
		}

		public static Bounds GetCombinedBounds(this IEnumerable<MeshFilter> meshes)
		{
			return meshes.Select(x => x.sharedMesh.bounds).ToArray().Combine();
		}
		
		public static Bounds GetCombinedBounds(this ICollection<MeshFilter> meshFilters)
		{
			if (meshFilters.Count == 0) return default(Bounds);
			
			var meshFilter = meshFilters.ElementAt(0);
			Bounds bounds = meshFilter.sharedMesh.bounds;
			// bounds.center += meshFilter.transform.localPosition;

			for (int i = 1; i < meshFilters.Count; ++i)
			{
				meshFilter = meshFilters.ElementAt(i);
				var b = meshFilter.sharedMesh.bounds;
				// b.center += meshFilter.transform.localPosition;
				bounds.Encapsulate(b);
			}
			
			return bounds;
		}
		
		public static Bounds GetCombinedBounds(this ICollection<Renderer> renderers)
		{
			if (renderers.Count == 0) return default(Bounds);
			
			var b = renderers.ElementAt(0).bounds;
			
			for (int i = 1; i < renderers.Count; ++i)
				b.Encapsulate(renderers.ElementAt(i).bounds);
			return b;
		}
		
		public static Bounds GetCombinedLocalBounds(this ICollection<Renderer> renderers, Transform axis)
		{
			if (renderers.Count == 0) return default(Bounds);
			
			var b = renderers.ElementAt(0).GetLocalBounds(axis);

			for (int i = 1; i < renderers.Count; ++i)
			{
				b.Encapsulate(renderers.ElementAt(i).GetLocalBounds(axis));
			}
			return b;
		}

		public static Bounds GetLocalBounds(this Renderer renderer, Transform axis)
		{
			var matrix = axis.worldToLocalMatrix * renderer.localToWorldMatrix;
			var b = renderer.bounds;

			if (renderer is MeshRenderer)
			{
				var filter = renderer.GetComponent<MeshFilter>();
				if (filter.sharedMesh)
					b = filter.sharedMesh.bounds;
			}
			
			var center = matrix.MultiplyPoint(b.center);
			var size = matrix.MultiplyVector(b.size);
			return new Bounds(center, size);
		}

		public static float GetVolume(this IEnumerable<MeshFilter> meshes)
		{
			return meshes.GetCombinedBounds().GetVolume();
		}
		
		public static Bounds GetCombinedBounds(this IEnumerable<Mesh> meshes)
		{
			return meshes.Select(x => x.bounds).ToArray().Combine();
		}

		public static float GetVolume(this IEnumerable<Mesh> meshes)
		{
			return meshes.GetCombinedBounds().GetVolume();
		}

		public static Bounds? GetOverlapWith(this Bounds b, Bounds o)
		{
			if (!b.Intersects(o))
				return null;

			var min = Vector3.Max(b.min, o.min);
			var max = Vector3.Min(b.max, o.max);

			var overlap = new Bounds();
			overlap.SetMinMax(min, max);
			return overlap;
		}
    }
}