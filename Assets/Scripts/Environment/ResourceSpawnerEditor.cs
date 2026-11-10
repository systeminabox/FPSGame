using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Environment.Editor
{
    // Fully qualified base class: inside a namespace ending in ".Editor",
    // a bare "Editor" resolves to the namespace and won't compile.
    [CustomEditor(typeof(ResourceSpawner))]
    public class ResourceSpawnerEditor : UnityEditor.Editor
    {
        private const string ContainerName = "Generated Resources";
        private const float DebugAxisLength = 2f;

        private class PreviewState
        {
            public List<ScatterPoint> points = new();
            public bool showDebug = true;
        }

        // Editor-only state, keyed by instance ID so the inspector and the
        // gizmo drawer share it without anything serializing into the scene.
        

        private ResourceSpawner Spawner => (ResourceSpawner)target;
        

        private static readonly Dictionary<ResourceSpawner, PreviewState> Previews = new();

        private static PreviewState GetState(ResourceSpawner spawner)
        {
            if (!Previews.TryGetValue(spawner, out PreviewState state))
            {
                state = new PreviewState();
                Previews[spawner] = state;
            }
            return state;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            PreviewState state = GetState(Spawner);

            EditorGUI.BeginChangeCheck();
            state.showDebug = EditorGUILayout.Toggle("Show Debug", state.showDebug);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            EditorGUILayout.LabelField("Preview Points", state.points.Count.ToString());

            if (GUILayout.Button("Generate Preview"))
            {
                state.points = new ScatterGenerator(Spawner).Generate();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Bake"))
            {
                Bake(state);
            }

            if (GUILayout.Button("Clear Baked"))
            {
                ClearBaked();
            }
        }

        private void Bake(PreviewState state)
        {
            // Regenerate rather than trusting the preview cache, which may be stale.
            List<ScatterPoint> points = new ScatterGenerator(Spawner).Generate();
            state.points = points;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Bake Resources");
            int undoGroup = Undo.GetCurrentGroup();

            ClearBaked();
            Transform container = CreateContainer();

            foreach (ScatterPoint point in points)
            {
                ResourceBody prefab = point.entry.profile.resourcePrefab;
                if (prefab == null) continue;

                // InstantiatePrefab keeps the link to the source prefab,
                // unlike a plain Instantiate.
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, container);
                Undo.RegisterCreatedObjectUndo(instance, "Bake Resources");

                // Name encodes entry + node index, which is what the stable node ID gets built from.
                instance.name = $"{prefab.name}_{point.entryIndex}_{point.index}";

                Transform t = instance.transform;
                t.SetPositionAndRotation(point.position, point.rotation);
                // Multiply, don't overwrite: the prefab may have its own authored scale.
                t.localScale = Vector3.Scale(prefab.transform.localScale, point.scale);
            }

            // One Ctrl+Z reverts the whole bake.
            Undo.CollapseUndoOperations(undoGroup);
            SceneView.RepaintAll();
        }

        private void ClearBaked()
        {
            Transform container = Spawner.transform.Find(ContainerName);
            if (container != null) Undo.DestroyObjectImmediate(container.gameObject);
        }

        private Transform CreateContainer()
        {
            var go = new GameObject(ContainerName);
            Undo.RegisterCreatedObjectUndo(go, "Bake Resources");
            go.transform.SetParent(Spawner.transform, false);
            return go.transform;
        }

        // Runs for any selected ResourceSpawner, no OnSceneGUI needed.
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawPreview(ResourceSpawner spawner, GizmoType gizmoType)
        {
            if (!Previews.TryGetValue(spawner, out PreviewState state)) return;
            if (!state.showDebug) return;

            foreach (ScatterPoint point in state.points)
            {
                Color color = point.entry.profile.gizmoColor;
                color.a = 1f; // a new Color field defaults to alpha 0, which is invisible
                Gizmos.color = color;
                Gizmos.DrawSphere(point.position, 0.3f * point.scale.x);

                Vector3 p = point.position;
                float len = point.scale.x * DebugAxisLength;

                Gizmos.color = Color.green; // up: alignment + scale
                Gizmos.DrawLine(p, p + point.rotation * Vector3.up * len);

                Gizmos.color = Color.blue;  // forward: yaw
                Gizmos.DrawLine(p, p + point.rotation * Vector3.forward * len);
            }
        }
    }
}