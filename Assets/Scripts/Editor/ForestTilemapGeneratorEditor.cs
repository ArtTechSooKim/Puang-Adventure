using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor for ForestTilemapGenerator
/// Adds easy-to-use buttons in the Inspector for generating and clearing tilemaps
/// </summary>
[CustomEditor(typeof(ForestTilemapGenerator))]
public class ForestTilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);

        ForestTilemapGenerator generator = (ForestTilemapGenerator)target;

        // Generate button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🌲 Generate Forest Map", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Generate Forest Map",
                "This will clear all existing tiles and generate a new forest map. Continue?",
                "Yes, Generate",
                "Cancel"))
            {
                Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Forest Map");
                generator.GenerateForestMap();
                EditorUtility.SetDirty(generator);
                SceneView.RepaintAll();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // Clear button
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("🧹 Clear All Tilemaps", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Tilemaps",
                "This will remove all tiles from all tilemaps. This action can be undone. Continue?",
                "Yes, Clear",
                "Cancel"))
            {
                Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Forest Tilemaps");
                generator.ClearAllTilemaps();
                EditorUtility.SetDirty(generator);
                SceneView.RepaintAll();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // Info box
        EditorGUILayout.HelpBox(
            "1. Assign all Tilemap references and Tile assets in the inspector above\n" +
            "2. Adjust portal positions and generation settings\n" +
            "3. Click 'Generate Forest Map' to create the forest\n" +
            "4. Further customize tiles manually in the Scene view",
            MessageType.Info);
    }
}

/// <summary>
/// Menu item for creating ForestTilemapGenerator from Unity menu
/// </summary>
public class ForestTilemapGeneratorMenu
{
    [MenuItem("GameObject/2D Object/Forest Tilemap Generator", false, 10)]
    private static void CreateForestGenerator(MenuCommand menuCommand)
    {
        // Find or create Grid
        GameObject gridObject = GameObject.Find("Grid");

        if (gridObject == null)
        {
            Debug.LogWarning("⚠ No Grid found in scene. Please add a Grid GameObject with Tilemaps first.");
            return;
        }

        // Check if generator already exists
        ForestTilemapGenerator existingGenerator = gridObject.GetComponent<ForestTilemapGenerator>();
        if (existingGenerator != null)
        {
            Debug.LogWarning("⚠ ForestTilemapGenerator already exists on Grid GameObject.");
            Selection.activeGameObject = gridObject;
            return;
        }

        // Add generator component
        ForestTilemapGenerator generator = gridObject.AddComponent<ForestTilemapGenerator>();

        // Try to auto-assign tilemaps
        Transform gridTransform = gridObject.transform;

        for (int i = 0; i < gridTransform.childCount; i++)
        {
            Transform child = gridTransform.GetChild(i);
            string childName = child.name.ToLower();

            var tilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap == null)
                continue;

            // Use reflection to set private fields (for auto-assignment convenience)
            var generatorType = typeof(ForestTilemapGenerator);

            if (childName.Contains("background"))
            {
                var field = generatorType.GetField("backgroundTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                Debug.Log($"✅ Auto-assigned: {child.name} → backgroundTilemap");
            }
            else if (childName.Contains("ground") && !childName.Contains("collision"))
            {
                var field = generatorType.GetField("groundTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                Debug.Log($"✅ Auto-assigned: {child.name} → groundTilemap");
            }
            else if (childName.Contains("collision"))
            {
                var field = generatorType.GetField("collisionTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                Debug.Log($"✅ Auto-assigned: {child.name} → collisionTilemap");
            }
            else if (childName.Contains("walkbehind"))
            {
                var field = generatorType.GetField("walkBehindTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                Debug.Log($"✅ Auto-assigned: {child.name} → walkBehindTilemap");
            }
            else if (childName.Contains("deco"))
            {
                var field = generatorType.GetField("decoTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                Debug.Log($"✅ Auto-assigned: {child.name} → decoTilemap");
            }
        }

        // Select the Grid object
        Selection.activeGameObject = gridObject;

        Debug.Log("✅ ForestTilemapGenerator added to Grid! Please assign tile assets in the Inspector.");
    }

    [MenuItem("Tools/Puang Adventure/Setup Forest Tilemap Generator")]
    private static void SetupForestGenerator()
    {
        // Find Grid in current scene
        GameObject gridObject = GameObject.Find("Grid");

        if (gridObject == null)
        {
            EditorUtility.DisplayDialog(
                "Grid Not Found",
                "No Grid GameObject found in the current scene. Please open the Forest Scene first.",
                "OK");
            return;
        }

        // Add or get existing generator
        ForestTilemapGenerator generator = gridObject.GetComponent<ForestTilemapGenerator>();
        if (generator == null)
        {
            generator = gridObject.AddComponent<ForestTilemapGenerator>();
            Debug.Log("✅ ForestTilemapGenerator component added to Grid");
        }

        // Auto-assign tilemaps (same logic as above)
        Transform gridTransform = gridObject.transform;
        int assignedCount = 0;

        for (int i = 0; i < gridTransform.childCount; i++)
        {
            Transform child = gridTransform.GetChild(i);
            string childName = child.name.ToLower();

            var tilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap == null)
                continue;

            var generatorType = typeof(ForestTilemapGenerator);

            if (childName.Contains("background"))
            {
                var field = generatorType.GetField("backgroundTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                assignedCount++;
            }
            else if (childName.Contains("ground") && !childName.Contains("collision"))
            {
                var field = generatorType.GetField("groundTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                assignedCount++;
            }
            else if (childName.Contains("collision"))
            {
                var field = generatorType.GetField("collisionTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                assignedCount++;
            }
            else if (childName.Contains("walkbehind"))
            {
                var field = generatorType.GetField("walkBehindTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                assignedCount++;
            }
            else if (childName.Contains("deco"))
            {
                var field = generatorType.GetField("decoTilemap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(generator, tilemap);
                assignedCount++;
            }
        }

        Selection.activeGameObject = gridObject;

        EditorUtility.DisplayDialog(
            "Setup Complete",
            $"ForestTilemapGenerator setup complete!\n\n" +
            $"• Tilemaps auto-assigned: {assignedCount}/5\n" +
            $"• Next step: Assign tile assets in the Inspector\n" +
            $"• Then click 'Generate Forest Map' button",
            "OK");

        Debug.Log($"✅ Setup complete! {assignedCount} tilemaps auto-assigned. Please assign tile assets in Inspector.");
    }
}
