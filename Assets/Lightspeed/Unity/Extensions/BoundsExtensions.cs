using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static  class BoundsExtensions
    {
	    public static Vector3[] GetCorners(this Bounds bounds)
	    {
		    var min = bounds.min;
		    var max = bounds.max;
		    return new[]
		    {
			    min,
			    new Vector3(min.x, min.y, max.z),
			    new Vector3(min.x, max.y, min.z),
			    new Vector3(min.x, max.y, max.z),
				
			    new Vector3(max.x, min.y, min.z),
			    new Vector3(max.x, min.y, max.z),
			    new Vector3(max.x, max.y, min.z),
			    max,
		    };
	    }
	    
	    public static Vector3[] GetCornersTransformed(this Bounds bounds, Matrix4x4 matrix)
	    {
		    var min = bounds.min;
		    var max = bounds.max;
		    return new[]
		    {
			    matrix.MultiplyPoint(min),
			    matrix.MultiplyPoint(new Vector3(min.x, min.y, max.z)),
			    matrix.MultiplyPoint(new Vector3(min.x, max.y, min.z)),
			    matrix.MultiplyPoint(new Vector3(min.x, max.y, max.z)),
                
			    matrix.MultiplyPoint(new Vector3(max.x, min.y, min.z)),
			    matrix.MultiplyPoint(new Vector3(max.x, min.y, max.z)),
			    matrix.MultiplyPoint(new Vector3(max.x, max.y, min.z)),
			    matrix.MultiplyPoint(max),
		    };
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
        
        public static float GetVolume(this IEnumerable<Collider> colliders)
        {
	        return colliders.GetCombinedBounds().GetVolume();
        }
        
        public static float GetVolume(this IEnumerable<MeshFilter> meshes)
        {
	        return meshes.GetCombinedBounds().GetVolume();
        }
        
        public static float GetVolume(this IEnumerable<Mesh> meshes)
        {
	        return meshes.GetCombinedBounds().GetVolume();
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
		
		public static Bounds EncapsulateFull(this Bounds b, Bounds otherB)
		{
			var corners = otherB.GetCorners();
			foreach (var corner in corners)
				b.Encapsulate(corner);
			return b;
		}
		
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
		
		public static Bounds Combine(this ICollection<Bounds> bounds)
		{
			if (bounds.Count <= 1) return bounds.FirstOrDefault();

			var b = bounds.ElementAt(0);
			for (int i = 1; i < bounds.Count; ++i)
				b.Encapsulate(bounds.ElementAt(i));

			return b;
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

			return default(Bounds);
		}
		
		public static Bounds GetObjectLocalBounds(this GameObject go, bool calculateUsingVerts = false)
		{
			var colliders = go.GetComponentsInChildren<Collider>();
			if (colliders.Any())
			{
				var bounds = colliders.GetCombinedLocalBounds(go.transform);
				return bounds;
			}
			
			var renderers = go.GetComponentsInChildren<Renderer>();
			if (renderers.Any())
			{
				var bounds = renderers.GetCombinedLocalBounds(go.transform, calculateUsingVerts);
				return bounds;
			}

			return default(Bounds);
		}

		public static Vector3[] GetLocalBounds(this Renderer renderer, Transform axis, bool calculateUsingVerts = false)
		{
			var matrix = axis.worldToLocalMatrix;
			var b = renderer.bounds; // World space

			if (renderer is MeshRenderer)
			{
				var filter = renderer.GetComponent<MeshFilter>();
				if (filter != null && filter.sharedMesh != null)
				{
					matrix = matrix * renderer.localToWorldMatrix; // Adjust matrix to compensate for object space
					Mesh sharedMesh = filter.sharedMesh;
					if (calculateUsingVerts && sharedMesh.isReadable)
					{
						var verts = sharedMesh.vertices;
						if (verts.Length == 0)
						{
							b = sharedMesh.bounds; // Object space
						}
						else
						{
							b = new Bounds(verts[0], Vector3.zero);
							for (int i = 0; i < verts.Length; ++i)
							{
								var vert = verts[i];
								if (b.Contains(vert))
									continue;
								b.Encapsulate(vert);
							}
						}
					}
					else
					{
						b = sharedMesh.bounds; // Object space
					}
				}
			}

			return b.GetCornersTransformed(matrix);
		}

		public static Vector3[]  GetLocalBounds(this Collider collider, Transform axis)
		{
			var matrix = axis.worldToLocalMatrix;
			var b = collider.bounds; // World space
			return b.GetCornersTransformed(matrix);
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

		public static Bounds GetCombinedBounds(this IEnumerable<MeshFilter> meshes)
		{
			return meshes.Select(x => x.sharedMesh.bounds).ToArray().Combine();
		}
		
		public static Bounds GetCombinedBounds(this ICollection<MeshFilter> meshFilters)
		{
			if (meshFilters.Count == 0) return default(Bounds);
			
			var meshFilter = meshFilters.ElementAt(0);
			Bounds bounds = meshFilter.sharedMesh.bounds;

			for (int i = 1; i < meshFilters.Count; ++i)
			{
				meshFilter = meshFilters.ElementAt(i);
				var b = meshFilter.sharedMesh.bounds;
				bounds.Encapsulate(b);
			}
			
			return bounds;
		}
		
		public static Bounds GetCombinedBounds(this IEnumerable<Mesh> meshes)
		{
			return meshes.Select(x => x.bounds).ToArray().Combine();
		}
		
		public static Bounds GetCombinedBounds(this ICollection<Renderer> renderers)
		{
			if (renderers.Count == 0) return default(Bounds);
			
			var b = renderers.ElementAt(0).bounds;
			
			for (int i = 1; i < renderers.Count; ++i)
				b.Encapsulate(renderers.ElementAt(i).bounds);
			return b;
		}
		
		public static Bounds GetCombinedLocalBounds(this ICollection<Renderer> renderers, Transform axis, bool calculateUsingVerts = false)
		{
			if (renderers.Count == 0) return default(Bounds);
			
			var corners = renderers.ElementAt(0).GetLocalBounds(axis, calculateUsingVerts);
			var b = new Bounds(corners[0], Vector3.zero);
			foreach (var corner in corners)
				b.Encapsulate(corner);

			for (int i = 1; i < renderers.Count; ++i)
			{
				corners = renderers.ElementAt(i).GetLocalBounds(axis, calculateUsingVerts);
				foreach (var corner in corners)
					b.Encapsulate(corner);
			}
			return b;
		}
		
		public static Bounds GetCombinedLocalBounds(this ICollection<Collider> colliders, Transform axis)
		{
			if (colliders.Count == 0) return default(Bounds);
			
			var corners = colliders.ElementAt(0).GetLocalBounds(axis);
			var b = new Bounds(corners[0], Vector3.zero);
			foreach (var corner in corners)
				b.Encapsulate(corner);

			for (int i = 1; i < colliders.Count; ++i)
			{
				corners = colliders.ElementAt(i).GetLocalBounds(axis);
				foreach (var corner in corners)
					b.Encapsulate(corner);
			}
			return b;
		}
		
		/// <summary>
		/// Slice a Axisaligned bounds according to one of the cardinal axes
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="axis"></param>
		/// <param name="pointOnSlicePlane">Point on the slice plane</param>
		/// <param name="halfBounds1"></param>
		/// <param name="halfBounds2"></param>
		/// <param name="discardMargin"></param>
		/// <returns></returns>
		public static bool TrySliceBounds(this Bounds bounds, Axis axis, Vector3 pointOnSlicePlane, out Bounds halfBounds1, out Bounds halfBounds2, float discardMargin = 0.02f)
		{
			if (!axis.IsSingleFlag())
			{
				halfBounds1 = default;
				halfBounds2 = default;
				return false;
			}

			var firstCorners = new List<Vector3>();
			var secondCorners = new List<Vector3>();
			// Create slicing plane
			Vector3 normal = axis.ToVector(1.0f);
			var plane = new Plane(normal, pointOnSlicePlane);
			foreach (var corner in bounds.GetCorners())
			{
				float distanceToPlane = plane.GetDistanceToPoint(corner);
				// If the point is too close to the side of the bounds, ignore it
				if (Mathf.Abs(distanceToPlane) < discardMargin)
					continue;

				// Check on which side of the slicing plane the corner is
				if (distanceToPlane > 0.0f)
					firstCorners.Add(corner);
				else
					secondCorners.Add(corner);
			}

			// If not all corners were categorized / were not split symmetrically
			if (firstCorners.Count != 4 || secondCorners.Count != 4)
			{
				halfBounds1 = default;
				halfBounds2 = default;
				return false;
			}


			halfBounds1 = CreateBoundsFromCornersToPlane(firstCorners, plane);
			halfBounds2 = CreateBoundsFromCornersToPlane(secondCorners, plane);
			return true;
		}

		/// <summary>
		/// Extrude 4 corner points against a plane
		/// </summary>
		/// <param name="halfCorners"></param>
		/// <param name="plane">Plane perpendicular to the half corner set</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		private static Bounds CreateBoundsFromCornersToPlane(List<Vector3> halfCorners, Plane plane)
		{
			if (halfCorners == null || halfCorners.Count != 4)
				throw new ArgumentException($"{nameof(halfCorners)} needs to have a length of 4.");
			Bounds b = new Bounds(halfCorners[0], Vector3.zero);
			for (int i = 1; i < 4; ++i)
				b.Encapsulate(halfCorners[i]);
			foreach (var corner in halfCorners)
			{
				// Project corner point on (slicing) plane
				Vector3 otherPoint = plane.ClosestPointOnPlane(corner);
				b.Encapsulate(otherPoint);
			}

			return b;
		}
	}
}