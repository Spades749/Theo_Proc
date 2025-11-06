using UnityEngine;
using UnityEditor;

namespace Components.ProceduralGeneration
{
    [CustomEditor(typeof(ProceduralGridGenerator))]
    public class ProceduralGridGeneratorEditor : Editor
    {
        private Editor _generationMethodEditor;
        
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);

            // Update the serialized object
            serializedObject.Update();
            
            // Display the _generationMethod field
            SerializedProperty generationMethodProp = serializedObject.FindProperty("_generationMethod");
            
            // Display all fields inside the ScriptableObject
            if (generationMethodProp.objectReferenceValue != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Generation Method Settings", EditorStyles.boldLabel);
                
                // Create a nested editor for the ScriptableObject
                CreateCachedEditor(generationMethodProp.objectReferenceValue, null, ref _generationMethodEditor);
                _generationMethodEditor.OnInspectorGUI();
            }
            
            // Apply changes to the serialized object
            serializedObject.ApplyModifiedProperties();
            
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