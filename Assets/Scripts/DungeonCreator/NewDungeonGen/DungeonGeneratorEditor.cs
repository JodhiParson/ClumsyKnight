using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a "Generate Dungeon" button to the DungeonGenerator's Inspector,
/// so you can regenerate the layout in the Editor without entering Play mode.
///
/// IMPORTANT: this script must be placed inside a folder named "Editor"
/// anywhere in your Assets folder (e.g. Assets/Editor/DungeonGeneratorEditor.cs).
/// Unity uses that folder name to know this script should only run in the Editor.
/// </summary>
[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the normal Inspector fields (width, height, prefabs, etc).
        DrawDefaultInspector();

        DungeonGenerator generator = (DungeonGenerator)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate Dungeon", GUILayout.Height(30)))
        {
            generator.GenerateDungeon();

            // Mark the scene dirty so Unity knows to prompt a save,
            // since the generated children count as a scene change.
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
            }
        }

        if (GUILayout.Button("Clear Dungeon", GUILayout.Height(24)))
        {
            generator.ClearDungeonPublic();

            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
            }
        }
    }
}