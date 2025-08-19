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

        public void RefreshEntries(bool clear = false)
        {
            var root = MeshSimplifierUtil.GetAvatarRoot(transform);
            if (root == null) return;

            if (clear)
            {
                Entries.Clear();
            }

            // Remove entries whose renderer is no longer valid
            Entries.RemoveAll(entry => !IsValidTarget(entry.Renderer));

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
        public bool PropertiesExpanded = false;

        public bool Enabled = true;
        //public bool Fixed = false;
        public Renderer Renderer;
        public float quality = 1.0f;
        public int OriginalTriangleCount => MeshSimplifierUtil.GetMesh(Renderer)?.GetTriangleCount() ?? 0;
        //public int TargetTriangleCount = -1;

        /// <summary>
        /// If you want to suppress hole generation during simplification, enable this option.
        /// </summary>
        [Tooltip("If you want to suppress hole generation during simplification, enable this option.")]
        public bool PreserveBorderEdges = false;
        public bool PreserveSurfaceCurvature = false;
        /// <summary>
        /// If you find that the texture is distorted, try toggling this option.
        /// </summary>
        [Tooltip("If you find that the texture is distorted, try toggling this option.")]
        public bool UseBarycentricCoordinateInterpolation = false;
        /// <summary>
        /// If this option is enabled, vertices that are not originally connected but are close to each other will be included in the first merge candidates. <br/>
        /// Increases the initialization cost.
        /// </summary>
        [Tooltip("If this option is enabled, vertices that are not originally connected but are close to each other will be included in the first merge candidates. \n" +
            "Increases the initialization cost.")]
        public bool EnableSmartLink = true;
        [Range(-1, 1)]
        public float MinNormalDot = 0.2f;
        /// <summary>
        /// When smart link is enabled, this is used to select candidates for merging vertices that are not originally connected to each other. <br/>
        /// Increasing this value also increases the initialization cost.
        /// </summary>
        [Tooltip("When smart link is enabled, this is used to select candidates for merging vertices that are not originally connected to each other. \n" +
            "Increasing this value also increases the initialization cost.")]
        public float VertexLinkDistance = 0.0001f;
        [Range(-1, 1)]
        public float VertexLinkMinNormalDot = 0.95f;
        // This could be HDR color, so there is no Range.
        public float VertexLinkColorDistance = 0.01f;
        [Range(0, 1.41421356237f)]
        public float VertexLinkUvDistance = 0.001f;

    }
}
