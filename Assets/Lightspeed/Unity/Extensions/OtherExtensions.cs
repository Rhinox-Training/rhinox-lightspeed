using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
	public static partial class OtherExtensions
	{
		/// <summary>
		/// Used to determine whether text displayed on this color should be white or black.
		/// Luminant colors work best with black.
		/// </summary>
		public static bool IsLuminant(this Color col)
		{
			return (col.r * 0.2126f + col.g * 0.7152f + col.b * 0.0722f) > (179f / 255);
		}
		
		public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
		{
			return new Color(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
		}

		public static void SetColor(this Material material, float? r = null, float? g = null, float? b = null,
			float? a = null)
		{
			material.color = material.color.With(r: r, g: g, b: b, a: a);
		}

		public static void SetAllMaterials(this Renderer renderer, Material mat)
		{
			var length = renderer.sharedMaterials.Length;
			var mats = new Material[length];
			for (int i = 0; i < length; ++i)
				mats[i] = mat;
			renderer.materials = mats;
		}

		public static void ReplaceCoroutine(this IEnumerator coroutine, ref Coroutine coroutineRef,
			MonoBehaviour caller)
		{
			if (coroutineRef != null)
				caller.StopCoroutine(coroutineRef);

			coroutineRef = caller.StartCoroutine(coroutine);
		}
		
		/// <summary>
		/// Returns the default value of type U if the key does not exist in the dictionary
		/// </summary>
		public static bool HasParameter(this Animator animator, string paramName)
		{
			return animator.parameters.Select(parameter => parameter.name).Contains(paramName);
		}
		
		/// <summary>
		/// returns the position of an element in a 2 dimensional array
		/// returns null if not present in the matrix
		/// NOTE: DO NOT PASS A NULL AS VALUE (since .Equals is called on it)
		/// </summary>
		public static Vector2Int? PositionOf<T>(this T[,] matrix, T value)
		{
			int w = matrix.GetLength(0);
			int h = matrix.GetLength(1);

			for (int x = 0; x < w; ++x)
			{
				for (int y = 0; y < h; ++y)
				{
					if (value.Equals(matrix[x, y]))
						return new Vector2Int(x, y);
				}
			}

			return null;
		}
	}
}
