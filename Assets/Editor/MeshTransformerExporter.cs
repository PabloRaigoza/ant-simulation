using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshTransformerExporter : EditorWindow
{
    // Transformation parameters
    private Vector3 positionOffset = Vector3.zero;
    private Vector3 rotationOffset = Vector3.zero;
    private Vector3 scaleMultiplier = Vector3.one;
    private string exportPath = "Assets/Meshes";

    // Selected GameObject
    private GameObject selectedObject;

    [MenuItem("Tools/Transform and Export Mesh")]
    public static void ShowWindow()
    {
        GetWindow<MeshTransformerExporter>("Mesh Transformer Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Transformation Settings", EditorStyles.boldLabel);

        // Select GameObject
        selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            GUILayout.Label("No GameObject selected. Please select a GameObject with a MeshFilter.");
            return;
        }

        MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            GUILayout.Label("Selected GameObject does not have a MeshFilter with a mesh.");
            return;
        }

        EditorGUILayout.Space();

        // Transformation fields
        positionOffset = EditorGUILayout.Vector3Field("Position Offset", positionOffset);
        rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset (Degrees)", rotationOffset);
        scaleMultiplier = EditorGUILayout.Vector3Field("Scale Multiplier", scaleMultiplier);

        EditorGUILayout.Space();

        // Export path
        exportPath = EditorGUILayout.TextField("Export Path", exportPath);
        if (!Directory.Exists(exportPath))
        {
            GUILayout.Label("Export path does not exist. It will be created.");
        }

        EditorGUILayout.Space();

        // Export button
        if (GUILayout.Button("Transform and Export Mesh"))
        {
            TransformAndExportMesh(meshFilter);
        }
    }

    private void TransformAndExportMesh(MeshFilter meshFilter)
    {
        // Clone the original mesh to avoid modifying it
        Mesh originalMesh = meshFilter.sharedMesh;
        Mesh transformedMesh = Instantiate(originalMesh);

        // Apply transformations
        Matrix4x4 transformationMatrix = Matrix4x4.TRS(positionOffset, Quaternion.Euler(rotationOffset), Vector3.Scale(originalMesh.bounds.size, scaleMultiplier));

        Vector3[] vertices = transformedMesh.vertices;
        Vector3[] normals = transformedMesh.normals;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transformationMatrix.MultiplyPoint3x4(vertices[i]);
            normals[i] = transformationMatrix.rotation * normals[i];
        }

        transformedMesh.vertices = vertices;
        transformedMesh.normals = normals;
        transformedMesh.RecalculateBounds();
        transformedMesh.RecalculateTangents();

        // Ensure export directory exists
        if (!AssetDatabase.IsValidFolder(exportPath))
        {
            Directory.CreateDirectory(exportPath);
            AssetDatabase.Refresh();
        }

        // Generate a unique name for the exported mesh
        string meshName = selectedObject.name + "_Transformed";
        string assetPath = Path.Combine(exportPath, meshName + ".asset");

        // Check if the mesh already exists
        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        if (existingMesh != null)
        {
            if (!EditorUtility.DisplayDialog("Mesh Exists",
                $"A mesh named '{meshName}' already exists. Do you want to overwrite it?",
                "Yes", "No"))
            {
                return;
            }

            // Overwrite the existing mesh
            EditorUtility.CopySerialized(transformedMesh, existingMesh);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Mesh '{meshName}' has been overwritten at {assetPath}.", "OK");
            return;
        }

        // Create the mesh asset
        AssetDatabase.CreateAsset(transformedMesh, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", $"Mesh '{meshName}' has been exported to {assetPath}.", "OK");
    }
}
