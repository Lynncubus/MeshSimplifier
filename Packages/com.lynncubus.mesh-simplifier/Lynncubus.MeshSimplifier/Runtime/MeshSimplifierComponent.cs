using System;
using System.Collections.Generic;
using System.Linq;
using Lynncubus.MeshSimplifier.Shared;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Lynncubus.MeshSimplifier.Runtime
{
    [AddComponentMenu("Lynn/Mesh Simplifier")]
    public class MeshSimplifierComponent : MonoBehaviour, IEditorOnly
    {
        public int TargetTriangleCount = 70000;
        public List<MeshSimplifierComponentEntry> Entries = new();

        public void RefreshEntries()
        {
            var root = MeshSimplifierUtil.GetAvatarRoot(transform);
            if (root == null) return;
            var currentRenderers = Entries.Select(x => x.Renderer).ToList();
            var addedRenderers = root.GetComponentsInChildren<Renderer>()
                .Except(currentRenderers)
                .Where(IsValidTarget)
                .Select(renderer => new MeshSimplifierComponentEntry { Renderer = renderer });

            Entries.AddRange(addedRenderers);
        }

        private bool IsValidTarget(Renderer renderer)
        {
            if (renderer == null) return false;
            if (MeshSimplifierUtil.IsEditorOnly(renderer.gameObject)) return false;
            if (renderer is not SkinnedMeshRenderer and not MeshRenderer) return false;
            var mesh = MeshSimplifierUtil.GetMesh(renderer);
            if (mesh == null || mesh.GetTriangleCount() == 0) return false;
            return true;
        }
    }

    [Serializable]
    public record MeshSimplifierComponentEntry
    {
        public bool Enabled = true;
        //public bool Fixed = false;
        public Renderer Renderer;
        public float quality = 1.0f;
        public int OriginalTriangleCount => MeshSimplifierUtil.GetMesh(Renderer)?.GetTriangleCount() ?? 0;
        //public int TargetTriangleCount = -1;
    }
}