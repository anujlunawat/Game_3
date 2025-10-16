using UnityEngine;
using UnityEditor;

public class AddMeshCollidersByMeshFilter : MonoBehaviour
{
    [MenuItem("Tools/Add Mesh Colliders (by Mesh Filter)")]
    static void AddMeshColliders()
    {
        // Get all MeshFilter components in the selected prefab or hierarchy
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>(true);

        int count = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            // Skip if no mesh assigned
            if (mf.sharedMesh == null) continue;

            GameObject go = mf.gameObject;

            // Check if a MeshCollider already exists
            MeshCollider mc = go.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = go.AddComponent<MeshCollider>();
                count++;
            }

            // Assign the same mesh
            mc.sharedMesh = mf.sharedMesh;

            // Optional: You can set Convex to false for static geometry
            mc.convex = false;
        }

        Debug.Log($"Added/Updated Mesh Colliders to {count} objects under '{Selection.activeGameObject.name}'.");
    }
}

