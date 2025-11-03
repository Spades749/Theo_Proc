using UnityEngine;
using UnityEditor;

namespace Components.ProceduralGeneration
{
    [CustomEditor(typeof(ProceduralGridGenerator))]
    public class ProceduralGridGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Add some space
            EditorGUILayout.Space(10);

            // Get the target component
            ProceduralGridGenerator generator = (ProceduralGridGenerator)target;

            if (GUILayout.Button("Debug Grid", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    generator.Grid.DrawGridDebug();
                }
                else
                {
                    EditorGUILayout.HelpBox("Grid generation can only be run in Play Mode!", MessageType.Warning);
                }
            }
            
            if (GUILayout.Button("Clear Grid", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    generator.ClearGrid();
                }
                else
                {
                    EditorGUILayout.HelpBox("Grid generation can only be run in Play Mode!", MessageType.Warning);
                }
            }


            if (GUILayout.Button("Generate Grid", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    generator.GenerateGrid();
                }
                else
                {
                    EditorGUILayout.HelpBox("Grid generation can only be run in Play Mode!", MessageType.Warning);
                }
            }


            // Show a helpful message when not in play mode
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use the Generate Grid button.", MessageType.Info);
            }
        }
    }
}