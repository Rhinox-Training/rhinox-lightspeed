using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        /// <summary>
        /// returns an inverted mask of the given layerNames
        /// </summary>
        public static LayerMask GetMaskExcluding(params string[] layerNames)
        {
            return ~(LayerMask.GetMask(layerNames));
        }

        /// <summary>
        /// returns a mask of the given layerIndices
        /// </summary>
        public static LayerMask GetMask(params int[] layerIndices)
        {
            return LayerMask.GetMask(layerIndices.Select(LayerMask.LayerToName).ToArray());
        }

        /// <summary>
        /// returns a mask of the given layerIndices
        /// </summary>
        public static LayerMask GetMask(params string[] layerNames)
        {
            return LayerMask.GetMask(layerNames);
        }

        /// <summary>
        /// returns an inverted mask of the given layerIndices
        /// </summary>
        public static LayerMask GetMaskExcluding(params int[] layerIndices)
        {
            return ~(GetMask(layerIndices));
        }
    }

    public static class LayermaskExtensions
    {
        /// <summary>
        /// Returns a copy of the Layermask with the layer added. Does not alter original
        /// </summary>
        public static LayerMask Add(this LayerMask mask, int layer)
        {
            mask |= (1 << layer);
            return mask;
        }

        /// <summary>
        /// Returns a copy of the Layermask with the layer added. Does not alter original
        /// </summary>
        public static LayerMask Add(this LayerMask mask, string layerName)
        {
            mask |= (1 << LayerMask.NameToLayer(layerName));
            return mask;
        }

        /// <summary>
        /// Returns a merged version of the 2 LayerMasks. Alters neither.
        /// </summary>
        public static LayerMask MergedWith(this LayerMask mask, LayerMask otherMask)
        {
            return mask | otherMask;
        }

        /// <summary>
        /// Returns wether the LayerMask contains the given layer.
        /// </summary>
        public static bool HasLayer(this LayerMask mask, int layer)
        {
            return (mask & (1 << layer)) > 0;
        }

        /// <summary>
        /// Returns wether the LayerMask contains the given layer.
        /// </summary>
        public static bool HasLayer(this LayerMask mask, string layerName)
        {
            return (mask & (1 << LayerMask.NameToLayer(layerName))) > 0;
        }
    }
}