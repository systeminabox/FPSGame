
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CombineChildMeshes
{
    [MenuItem("Tools/Combine Child Meshes")]
    static void Combine()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogError("Select the prefab root first.");
            return;
        }

        MeshFilter[] filters =
            root.GetComponentsInChildren<MeshFilter>();

        List<CombineInstance> combines =
            new List<CombineInstance>();

        Material material = null;

        foreach (MeshFilter filter in filters)
        {
            if (filter.transform == root.transform)
                continue;

            MeshRenderer renderer =
                filter.GetComponent<MeshRenderer>();

            if (filter.sharedMesh == null ||
                renderer == null || !renderer.enabled)
                continue;

            if (material == null)
                material = renderer.sharedMaterial;

            combines.Add(new CombineInstance
            {
                mesh = filter.sharedMesh,
                transform = root.transform.worldToLocalMatrix *
                            filter.transform.localToWorldMatrix
            });
        }

        if (combines.Count == 0 || material == null)
        {
            Debug.LogError("No valid child meshes found.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Combined Mesh",
            "Anemone002_Combined",
            "asset",
            "Choose where to save the combined mesh."
        );

        if (string.IsNullOrEmpty(path))
            return;

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "Anemone002_Combined";
        combinedMesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;

        combinedMesh.CombineMeshes(
            combines.ToArray(), true, true
        );

        combinedMesh.RecalculateBounds();

        AssetDatabase.CreateAsset(combinedMesh, path);

        MeshFilter rootFilter =
            root.GetComponent<MeshFilter>();

        if (rootFilter == null)
            rootFilter = root.AddComponent<MeshFilter>();

        rootFilter.sharedMesh = combinedMesh;

        MeshRenderer rootRenderer =
            root.GetComponent<MeshRenderer>();

        if (rootRenderer == null)
            rootRenderer = root.AddComponent<MeshRenderer>();

        rootRenderer.sharedMaterial = material;
        rootRenderer.enabled = true;

        foreach (MeshFilter filter in filters)
        {
            if (filter.transform == root.transform)
                continue;

            MeshRenderer childRenderer =
                filter.GetComponent<MeshRenderer>();

            if (childRenderer != null)
                childRenderer.enabled = false;
        }

        EditorUtility.SetDirty(root);
        Debug.Log("Combined plant mesh successfully!");
    }
}
