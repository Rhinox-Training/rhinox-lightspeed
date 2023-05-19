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

	    public static IEnumerable<Vector3> Sample(this Bounds bounds, int increments = 3)
	    {
		    float stepX = bounds.size.x / increments;
		    float stepY = bounds.size.y / increments;
		    float stepZ = bounds.size.z / increments;

		    float samplePointBorderOffsetX = 0.5f * stepX;
		    float samplePointBorderOffsetY = 0.5f * stepZ;
		    float samplePointBorderOffsetZ = 0.5f * stepZ;

		    for (int i = 0; i < increments; ++i)
		    {
			    float x = bounds.center.x - bounds.extents.x + i * stepX + samplePointBorderOffsetX;

			    for (int j = 0; j < increments; ++j)
			    {
				    float y = bounds.center.y - bounds.extents.y + j * stepY + samplePointBorderOffsetY;
				    
				    for (int k = 0; k < increments; ++k)
				    {
					    float z = bounds.center.z - bounds.extents.z + k * stepZ + samplePointBorderOffsetZ;

					    Vector3 pt = new Vector3(x, y, z);
					    yield return pt;
				    }
			    }
		    }
	    }

	    public static IEnumerable<Vector3> Sample2D(this Bounds bounds, int increments = 3)
	    {
		    float stepX = bounds.size.x / increments;
		    float stepZ = bounds.size.z / increments;

		    float samplePointBorderOffsetX = 0.5f * stepX;
		    float samplePointBorderOffsetZ = 0.5f * stepZ;

		    for (int i = 0; i < increments; ++i)
		    {
			    float x = bounds.center.x - bounds.extents.x + i * stepX + samplePointBorderOffsetX;

			    for (int j = 0; j < increments; ++j)
			    {
				    float z = bounds.center.z - bounds.extents.z + j * stepZ + samplePointBorderOffsetZ;

				    Vector3 pt = new Vector3(x, bounds.center.y - bounds.extents.y, z);
				    yield return pt;
			    }
		    }
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
		
		public static Bounds GetObjectLocalBounds(this GameObject go, bool calculateUsingVerts = false, Renderer[] renderers = null, Collider[] colliders = null)
		{
			if (colliders == null) colliders = go.GetComponentsInChildren<Collider>();
			if (colliders.Any())
			{
				var bounds = colliders.GetCombinedLocalBounds(go.transform);
				return bounds;
			}
			
			if (renderers == null) renderers = go.GetComponentsInChildren<Renderer>();
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

		public enum Side
		{
			Negative,
			Center,
			Positive
		}

		public static Bounds Resize(this Bounds bounds, Axis axis, float newSize, Side resizeOrigin = Side.Center)
		{
			var size = bounds.size;
			var oldHalfSize = size / 2.0f;
			var center = bounds.center;

			var offset = resizeOrigin == Side.Center ? 0.0f : oldHalfSize.x - (newSize / 2.0f);
			bool isPositive = resizeOrigin != Side.Negative;
			offset = isPositive ? -offset : offset;
			
			switch (axis)
			{
				case Axis.X:
					size.x = newSize;
					center.x += offset;
					break;
				case Axis.Y:
					size.y = newSize;
					center.y += offset;
					break;
				case Axis.Z:
					size.z = newSize;
					center.z += offset;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
			}

			return new Bounds(center, size);
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

		public static void CenterObjectBoundsOnPosition(this GameObject go, Vector3 position, Quaternion? rotation = null)
		{
			var localBounds = go.GetObjectLocalBounds();
			Vector3 localPositionOfCenter = localBounds.center;

			Vector3 globalPositionOfCenter = go.transform.TransformPoint(localPositionOfCenter);
			Vector3 offset = go.transform.position - globalPositionOfCenter;

			Vector3 rotatedOffset = (rotation ?? Quaternion.identity) * offset;
			
			if (rotation != null)
				TransformExtensions.RotateAround(go.transform, globalPositionOfCenter, rotation.Value);
			go.transform.position = position + rotatedOffset;
		}
		
		public static Axis GetDominantAxis(this Bounds b)
		{
			if (b.size.z > b.size.x && b.size.z > b.size.y)
				return Axis.Z;
			if (b.size.y > b.size.x)
				return Axis.Y;
			return Axis.X;
		}

		public static float GetExtents(this Bounds bounds, Axis axis)
		{
			float offset = 0.0f;
			switch (axis)
			{
				case Axis.X:
					offset = bounds.extents.x;
					break;
				case Axis.Y:
					offset = bounds.extents.y;
					break;
				case Axis.Z:
					offset = bounds.extents.z;
					break;
				default:
					throw new ArgumentException("Axis is wrong range, only X,Y,Z are supported", nameof(axis));
			}

			return offset;
		}

		public static float GetSize(this Bounds bounds, Axis axis)
		{
			float offset = 0.0f;
			switch (axis)
			{
				case Axis.X:
					offset = bounds.size.x;
					break;
				case Axis.Y:
					offset = bounds.size.y;
					break;
				case Axis.Z:
					offset = bounds.size.z;
					break;
				default:
					throw new ArgumentException("Axis is wrong range, only X,Y,Z are supported", nameof(axis));
			}

			return offset;
		}
		
		public static float GetSmallestDimension(this Bounds bounds)
		{
			return Mathf.Min(Mathf.Min(bounds.size.x, bounds.size.y), bounds.size.z);
		}
		
		public static float GetLargestDimension(this Bounds bounds)
		{
			return Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
		}
		
		public static int CountDimensionsBiggerThan(this Bounds bounds, float length)
		{
			int dimCount = 0;
			if (bounds.size.x >= length)
				++dimCount;
			if (bounds.size.y >= length)
				++dimCount;
			if (bounds.size.z >= length)
				++dimCount;
			return dimCount;
		}
		
		public static Rect ToScreenSpace(this Bounds bounds, Camera camera)
		{
			var corners = bounds.GetCorners().Select(x => camera.WorldToScreenPoint(x)).ToArray();
			Rect screenbounds = new Rect(corners[0], Vector2.zero);

			for (int idx = 1; idx < corners.Length; ++idx)
			{
				screenbounds = screenbounds.Encapsulate(corners[idx]);
			}

			float oldYmin = screenbounds.yMin;
			screenbounds.yMin = Screen.height - screenbounds.yMax;
			screenbounds.yMax = Screen.height - oldYmin;

			return screenbounds;
		}

		public static Rect ToScreenSpace(this Bounds bounds, CameraSpoof cameraSpoof)
		{
			var corners = bounds.GetCorners().Select(x => cameraSpoof.WorldToScreenPoint(x)).ToArray();
			Rect screenbounds = new Rect(corners[0], Vector2.zero);

			for (int idx = 1; idx < corners.Length; ++idx)
			{
				screenbounds = screenbounds.Encapsulate(corners[idx]);
			}

			float oldYmin = screenbounds.yMin;
			screenbounds.yMin = Screen.height - screenbounds.yMax;
			screenbounds.yMax = Screen.height - oldYmin;

			return screenbounds;
		}
    
		public static float GetScreenPixels(this Bounds bounds, Camera camera)
		{
			var rect = ToScreenSpace(bounds, camera);
			return rect.width * rect.height;
		}

		public static float GetScreenPixels(this Bounds bounds, CameraSpoof cameraSpoof)
		{
			var rect = ToScreenSpace(bounds, cameraSpoof);
			return rect.width * rect.height;
		}

		public static Bounds AddMarginToExtends(this Bounds bounds, Vector3 margin)
		{
			bounds.extents += margin;
			return bounds;
		}

		public static Bounds AddMarginToExtends(this Bounds bounds, float margin)
		{
			return bounds.AddMarginToExtends(new Vector3(margin, margin, margin));
		}
		
		public static Vector3 GetCorner(this Bounds bound, bool maxNotMinFlipped, Axis axis)
		{
			switch (axis)
			{
				case Axis.X:
					if (!maxNotMinFlipped)
						return new Vector3(bound.max.x, bound.min.y, bound.min.z);
					else
						return new Vector3(bound.min.x, bound.max.y, bound.max.z);
				case Axis.Y:
					if (!maxNotMinFlipped)
						return new Vector3(bound.min.x, bound.max.y, bound.min.z);
					else
						return new Vector3(bound.max.x, bound.min.y, bound.max.z);
				case Axis.Z:
					if (!maxNotMinFlipped)
						return new Vector3(bound.min.x, bound.min.y, bound.max.z);
					else
						return new Vector3(bound.max.x, bound.max.y, bound.min.z);
				default:
					throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
			}
		}

		public static bool IsAxisAlignedWith(this Bounds bound, Bounds otherBound)
		{
			Vector3 centerAxis = (otherBound.center - bound.center).normalized;
			if (!centerAxis.TryGetCardinalAxis(out var axis))
				return false;
			return TestBoundAlignmentOnAxis(bound, otherBound, axis);
		}

		private static bool TestBoundAlignmentOnAxis(Bounds bound, Bounds otherBound, Axis axis)
		{
			Vector3 corner1 = bound.min;
			Vector3 corner2 = bound.GetCorner(false, axis);

			Vector3 lineSegmentA = corner2 - corner1;

			if (!lineSegmentA.IsColinear(otherBound.min - corner2))
				return false;

			Vector3 corner3 = bound.max;
			Vector3 corner4 = bound.GetCorner(true, axis);

			Vector3 lineSegmentB = corner4 - corner3;

			if (!lineSegmentB.IsColinear(otherBound.max - corner3))
				return false;
			return AreTouching(bound, otherBound, axis, 0.0001f);
		}

		private static bool AreTouching(Bounds bound, Bounds otherBound, Axis axis, float epsilon = float.Epsilon)
		{
			switch (axis)
			{
				case Axis.X:
					return CheckTouchingSegments(bound.min.x, bound.max.x, otherBound.min.x, otherBound.max.x, epsilon);
				case Axis.Y:
					return CheckTouchingSegments(bound.min.y, bound.max.y, otherBound.min.y, otherBound.max.y, epsilon);
				case Axis.Z:
					return CheckTouchingSegments(bound.min.z, bound.max.z, otherBound.min.z, otherBound.max.z, epsilon);
				default:
					throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
			}
		}

		private static bool CheckTouchingSegments(float minA, float maxA, float minB, float maxB, float epsilon = float.Epsilon)
		{
			return Mathf.Min(minA, maxA).LossyEquals(Mathf.Max(minB, maxB), epsilon) ||
			       Mathf.Max(minA, maxA).LossyEquals(Mathf.Min(minB, maxB), epsilon);
		}

	}
}