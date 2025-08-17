using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Lynncubus.MeshSimplifier.Shared
{
    public static class MeshSimplifierUtil
    {
        public static Transform GetAvatarRoot(Transform transform)
        {
            return transform.GetComponentInParent(typeof(VRCAvatarDescriptor))?.transform;
        }

        public static int GetTriangleCount(this Mesh.MeshData mesh)
        {
            var indexCount = 0;
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                var subMesh = mesh.GetSubMesh(subMeshIndex);
                if (subMesh.topology == MeshTopology.Triangles)
                {
                    indexCount += subMesh.indexCount;
                }
            }
            return indexCount / 3;
        }
        public static int GetTriangleCount(this Mesh mesh)
        {
            var indexCount = 0;
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                var subMesh = mesh.GetSubMesh(subMeshIndex);
                if (subMesh.topology == MeshTopology.Triangles)
                {
                    indexCount += subMesh.indexCount;
                }
            }
            return indexCount / 3;
        }

        public static Mesh GetMesh(Renderer renderer)
        {
            switch (renderer)
            {
                case MeshRenderer meshRenderer:
                    {
                        if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter))
                        {
                            var mesh = meshFilter.sharedMesh;
                            if (mesh == null) return null;
                            return mesh;
                        }
                        else
                        {
                            return null;
                        }
                    }
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    {
                        var mesh = skinnedMeshRenderer.sharedMesh;
                        if (mesh == null) return null;
                        return mesh;
                    }
                default:
                    throw new ArgumentException($"Unsupported type of renderer: {renderer.GetType()}");
            }
        }

        public static void SetMesh(Renderer renderer, Mesh mesh)
        {
            switch (renderer)
            {
                case MeshRenderer meshrenderer:
                    var meshfilter = meshrenderer.GetComponent<MeshFilter>();
                    if (meshfilter == null) throw new ArgumentException($"The associated renderer was {nameof(MeshRenderer)}, but it has no {nameof(MeshFilter)}.");
                    meshfilter.sharedMesh = mesh;
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.sharedMesh = mesh;
                    break;
                default:
                    throw new ArgumentException($"Unsupported type of renderer: {renderer.GetType()}");
            }
        }

        public static bool IsEditorOnly(GameObject gameObject)
        {
            if (gameObject == null) return false;
            Transform current = gameObject.transform;
            while (current != null)
            {
                if (current.CompareTag("EditorOnly"))
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }
}

