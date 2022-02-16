using System;
using UnityEngine;

namespace Rhinox.Utilities
{
    public static partial class Extensions
    {
        public static bool IsWithinFrustum(this Mesh mesh, Camera camera)
        {
            if (camera == null) return false;
            
            var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, mesh.bounds);
        }

        public static bool IsWithinFrustum(this Vector3 position, Camera camera)
        {
            if (camera == null) return false;
            
            var viewPos = camera.WorldToViewportPoint(position);
            return viewPos.x.IsBetween(0, 1) && viewPos.y.IsBetween(0, 1) && viewPos.z > 0;
        }

        // TODO: migrate to Lightspeed
        public static bool IsBetween(this float number, float min, float max)
        {
            if (min > max)
                Utility.Swap(ref min, ref max);
            
            return number > min && number < max;
        }
        
        public static void DrawCameraFrustum(this Camera camera)
        {
            var matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);
        
            if (camera.orthographic) 
            {
                var spread = camera.farClipPlane - camera.nearClipPlane;
                var center = (camera.farClipPlane + camera.nearClipPlane) * 0.5f;
                Gizmos.DrawWireCube(new Vector3(0, 0, center), new Vector3(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2, spread));
            } 
            else 
            {
                Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
            }
        
            Gizmos.matrix = matrix;
        }
    }
}