using System;
using UnityEditor;
using Lynncubus.MeshSimplifier.Runtime;
using Lynncubus.MeshSimplifier.Shared;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using System.Linq;

namespace Lynncubus.MeshSimplifier.Editor
{
    [CustomEditor(typeof(MeshSimplifierComponent))]
    public class MeshSimplifierComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Debug.Log("MeshSimplifierComponentEditor.OnInspectorGUI called");
            var my = (MeshSimplifierComponent)target;
            var root = MeshSimplifierUtil.GetAvatarRoot(my.transform);
            if (root == null)
            {
                EditorGUILayout.HelpBox("This component must be placed on an avatar root.", MessageType.Error);
                return;
            }

            my.RefreshEntries();

            var totalTriangleCount = 0;
            var adjustableTriangleCount = 0;

            foreach (var entry in my.Entries)
            {
                var triangleCount = entry.OriginalTriangleCount;
                totalTriangleCount += triangleCount;
                if (entry.Enabled)
                {
                    adjustableTriangleCount += triangleCount;
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Triangles", GUILayout.Width(120));
            my.TargetTriangleCount = EditorGUILayout.IntField(my.TargetTriangleCount);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Triangles", GUILayout.Width(120));
            EditorGUILayout.LabelField(totalTriangleCount.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Adjustable Triangles", GUILayout.Width(120));
            EditorGUILayout.LabelField(adjustableTriangleCount.ToString());
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Simplified Triangles", GUILayout.Width(120));
            //var simplifiedTriangleCount = my.Entries.Select(x => x.TargetTriangleCount).Sum();
            //EditorGUILayout.LabelField(simplifiedTriangleCount.ToString());
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            foreach (var entry in my.Entries)
            {
                var entryObject = serializedObject.FindProperty("Entries").GetArrayElementAtIndex(my.Entries.IndexOf(entry));

                EditorGUILayout.BeginHorizontal();
                entry.Enabled = EditorGUILayout.Toggle(entry.Enabled, GUILayout.Width(20));
                //entry.Fixed = EditorGUILayout.Toggle(entry.Fixed, GUILayout.Width(20));
                GUI.enabled = entry.Enabled;
                GUI.enabled = false;
                EditorGUILayout.ObjectField(entry.Renderer, typeof(Renderer), true, GUILayout.Width(100));
                GUI.enabled = entry.Enabled;
                entry.quality = EditorGUILayout.Slider(entry.quality, 0.0f, 1.0f);

                EditorGUILayout.LabelField($"Tris: {entry.OriginalTriangleCount}", GUILayout.Width(100));

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                entry.PropertiesExpanded = EditorGUILayout.Foldout(entry.PropertiesExpanded, "Properties", true);
                if (entry.PropertiesExpanded)
                {
                    entry.PreserveBorderEdges = EditorGUILayout.Toggle("Preserve Border Edges", entry.PreserveBorderEdges);
                    entry.PreserveSurfaceCurvature = EditorGUILayout.Toggle("Preserve Surface Curvature", entry.PreserveSurfaceCurvature);
                    entry.UseBarycentricCoordinateInterpolation = EditorGUILayout.Toggle("Use Barycentric Coordinate Interpolation", entry.UseBarycentricCoordinateInterpolation);
                    EditorGUILayout.Space();
                    entry.EnableSmartLink = EditorGUILayout.Toggle("Enable Smart Link", entry.EnableSmartLink);
                    if (entry.EnableSmartLink)
                    {
                        entry.VertexLinkDistance = EditorGUILayout.FloatField("Vertex Link Distance", entry.VertexLinkDistance);
                        entry.VertexLinkMinNormalDot = EditorGUILayout.Slider("Vertex Link Min Normal Dot", entry.VertexLinkMinNormalDot, 0.0f, 1.0f);
                        entry.VertexLinkColorDistance = EditorGUILayout.FloatField("Vertex Link Color Distance", entry.VertexLinkColorDistance);
                        entry.VertexLinkUvDistance = EditorGUILayout.Slider("Vertex Link UV Distance", entry.VertexLinkUvDistance, 0, 1.41421356237f);
                    }
                }
            }

            if (GUILayout.Button("Reset Entries"))
            {
                my.RefreshEntries(true);
            }

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
