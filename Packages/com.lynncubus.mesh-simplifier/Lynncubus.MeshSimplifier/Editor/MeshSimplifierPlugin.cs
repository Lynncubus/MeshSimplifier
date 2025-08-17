using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lynncubus.MeshSimplifier.Editor;
using Lynncubus.MeshSimplifier.Runtime;
using Lynncubus.MeshSimplifier.Shared;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using MeshiaStuff = Meshia.MeshSimplification;

[assembly: ExportsPlugin(typeof(MeshSimplifierPlugin))]

namespace Lynncubus.MeshSimplifier.Editor
{
    public class MeshSimplifierPlugin : Plugin<MeshSimplifierPlugin>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("Simplify Meshes", ctx =>
                {
                    var components = ctx.AvatarRootObject.GetComponentsInChildren<MeshSimplifierComponent>();
                    if (components.Length == 0) return;
                    if (components.Length > 1)
                    {
                        Debug.LogWarning("Multiple MeshSimplifierComponents found. Only the first one will be used.");
                    }
                    var component = components[0];

                    component.RefreshEntries();

                    var globalTargetTriangleCount = component.TargetTriangleCount;

                    var targets = new List<MeshSimplifierTarget>();

                    foreach (var entry in component.Entries)
                    {
                        var target = new MeshSimplifierTarget
                        {
                            Entry = entry,
                            originalMesh = MeshSimplifierUtil.GetMesh(entry.Renderer),
                            triangleCount = entry.OriginalTriangleCount,
                        };
                        targets.Add(target);
                    }

                    // Distribute target triangle counts proportionally
                    for (int i = 0; i < 5; i++)
                    {
                        var currentTotal = targets.Sum(t => t.triangleCount);
                        var adjustableTotal = targets.Where(t => t.Entry.Enabled).Sum(t => t.triangleCount);

                        if (adjustableTotal == 0) break; // No adjustable triangles left

                        var adjustableTargetCount = globalTargetTriangleCount - (currentTotal - adjustableTotal);
                        if (adjustableTargetCount <= 0) break; // No more triangles to adjust

                        var proportion = (float)adjustableTargetCount / adjustableTotal;
                        foreach (var target in targets)
                        {
                            if (!target.Entry.Enabled) continue;

                            var originalTriangleCount = target.Entry.OriginalTriangleCount;
                            var currentTriangleCount = target.triangleCount;

                            var newTriangleCount = Mathf.Clamp((int)(currentTriangleCount * (proportion * target.Entry.quality)), 0, originalTriangleCount);
                            target.triangleCount = newTriangleCount;
                        }
                    }

                    foreach (var target in targets)
                    {
                        Debug.Log($"Simplifying {target.Entry.Renderer.name} from {target.Entry.OriginalTriangleCount} triangles to {target.triangleCount} triangles.");

                        target.targetMesh = new Mesh();
                        target.targetMesh.name = $"{target.Entry.Renderer.name}_Simplified";
                    }
                    var simplifiedTriangleCount = targets.Sum(t => t.triangleCount);
                    Debug.Log($"Total simplified triangles: {simplifiedTriangleCount}");

                    using (ListPool<(Mesh Mesh, MeshiaStuff.MeshSimplificationTarget Target, MeshiaStuff.MeshSimplifierOptions Options, Mesh Destination)>.Get(out var parameters))
                    {
                        foreach (var target in targets)
                        {
                            if (!target.Entry.Enabled) continue;

                            var meshiaTarget = new MeshiaStuff.MeshSimplificationTarget
                            {
                                Kind = MeshiaStuff.MeshSimplificationTargetKind.AbsoluteTriangleCount,
                                Value = target.triangleCount
                            };
                            var meshiaOptions = new MeshiaStuff.MeshSimplifierOptions
                            {
                                PreserveBorderEdges = true,
                            };

                            parameters.Add((target.originalMesh, meshiaTarget, meshiaOptions, target.targetMesh));
                        }

                        MeshiaStuff.MeshSimplifier.SimplifyBatch(parameters);

                        foreach (var target in targets)
                        {
                            if (!target.Entry.Enabled) continue;
                            var renderer = target.Entry.Renderer;
                            AssetDatabase.AddObjectToAsset(target.targetMesh, ctx.AssetContainer);
                            MeshSimplifierUtil.SetMesh(renderer, target.targetMesh);
                        }

                        UnityEngine.Object.DestroyImmediate(component);
                    }
                });
        }
    }

    public record MeshSimplifierTarget
    {
        public MeshSimplifierComponentEntry Entry;
        public Mesh originalMesh;
        public Mesh targetMesh;
        public int triangleCount;
    }
}