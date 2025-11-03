using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTools.Grid;

namespace VTools.ScriptableObjectDatabase
{
    public static class ScriptableObjectDatabase
    {
        private static readonly Dictionary<Type, Dictionary<string, ScriptableObject>> DATABASE = new();
        
        // -------------------------------------- DATA BASE CONSTRUCTION ---------------------------------------------
        
        // This static constructor will be called at the first time GetScriptableObject is called.
        static ScriptableObjectDatabase()
        {
            LoadAllScriptableObjects<GridObjectTemplate>();
        }

        private static void LoadAllScriptableObjects<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            
            if (!DATABASE.ContainsKey(type))
            {
                DATABASE[type] = new Dictionary<string, ScriptableObject>();
            }

            T[] templates = Resources.LoadAll<T>("");
            foreach (var template in templates)
            {
                DATABASE[type][template.name] = template;
            }
        }

        public static T GetScriptableObject<T>(string name) where T : ScriptableObject
        {
            var type = typeof(T);
            if (DATABASE.TryGetValue(type, out var typeDictionary))
            {
                if (typeDictionary.TryGetValue(name, out var scriptableObject))
                {
                    return scriptableObject as T;
                }
            }
        
            Debug.LogError($"Unable to find a {type.Name} with name: {name}");
            return null;
        }
        
        public static T GetFirstScriptableObject<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            if (DATABASE.TryGetValue(type, out var typeDictionary))
            {
                if (typeDictionary.Count > 0)
                {
                    return typeDictionary.Values.First() as T;
                }
            }
        
            Debug.LogError($"Unable to find a {type.Name}.");
            return null;
        }

        public static T GetRandomScriptableObject<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            if (DATABASE.TryGetValue(type, out var typeDictionary))
            {
                if (typeDictionary.Count > 0)
                {
                    var randomIndex = UnityEngine.Random.Range(0, typeDictionary.Count);
                    return typeDictionary.Values.ElementAt(randomIndex) as T;
                }
            }
        
            Debug.LogError($"Unable to find a random SO of type: {type.Name}.");
            return null;
        }
        
        public static List<T> GetAllScriptableObjectOfType<T>() where T : ScriptableObject
        {
            var result = new List<T>();

            var type = typeof(T);
            if (DATABASE.TryGetValue(type, out var typeDictionary))
            {
                foreach (var scriptableObject in typeDictionary)
                {
                    result.Add(scriptableObject.Value as T);
                }
            }

            return result;
        }
    }
}