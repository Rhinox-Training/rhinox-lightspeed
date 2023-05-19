using UnityEngine;

namespace Rhinox.Lightspeed
{
    public struct MeshInfo
    {
        public Mesh Mesh;
        public Bounds RendererBounds;

        public static bool TryCreate(Renderer renderer, out MeshInfo meshInfo)
        {
            meshInfo = new MeshInfo();
            if (renderer is MeshRenderer meshRenderer)
            {
                var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    return false;
                meshInfo.Mesh = meshFilter.sharedMesh;
                meshInfo.RendererBounds = meshRenderer.bounds;
                return true;
            }
            else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                meshInfo.Mesh = skinnedMeshRenderer.sharedMesh;
                meshInfo.RendererBounds = skinnedMeshRenderer.bounds;
                return true;
            }
            return false;
        }

        public static MeshInfo Create(Renderer renderer)
        {
            if (TryCreate(renderer, out MeshInfo mi))
                return mi;
            return default(MeshInfo);
        }
    }
}