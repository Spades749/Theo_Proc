using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace VTools.Utility
{
    public static class ExtensionMethods
    {
        public static void SetAlpha(this UnityEngine.UI.Graphic graphic, float value)
        {
            Color c = graphic.color;
            c.a = value;
            graphic.color = c;
        }
        
        public static Quaternion ClampAxis(this Quaternion quaternion, Axis axis, float minAngle, float maxAngle)
        {
            Vector3 euler = quaternion.eulerAngles;

            switch (axis)
            {
                case Axis.X:
                    euler.x = ClampAngle(euler.x, minAngle, maxAngle);
                    break;
                case Axis.Y:
                    euler.y = ClampAngle(euler.y, minAngle, maxAngle);
                    break;
                case Axis.Z:
                    euler.z = ClampAngle(euler.z, minAngle, maxAngle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            return Quaternion.Euler(euler);
        }

        // Helper function to handle angle wrapping
        private static float ClampAngle(float angle, float min, float max)
        {
            // Normalize angle to the range [-180, 180]
            angle = (angle > 180) ? angle - 360 : angle;

            return Mathf.Clamp(angle, min, max);
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public static float NormalizeAngle(this float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        public static int NormalizeAngle(this int angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }
        
        public static Color GenerateRandomColorWithAlpha(float alpha)
        {
            var r = UnityEngine.Random.Range(0f, 1f);
            var g = UnityEngine.Random.Range(0f, 1f);
            var b = UnityEngine.Random.Range(0f, 1f);

            return new Color(r, g, b, alpha);
        }

        /// <summary>
        /// Populates the dropdown with enum values
        /// </summary>
        public static void PopulateDropdownFromEnum<T>(this TMP_Dropdown dropdown) where T : Enum
        {
            // Clear existing options
            dropdown.ClearOptions();

            // Get all enum values and convert to strings
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>();
            var optionsList = enumValues.Select(value => value.ToString().ToReadableFromEnum()).ToList();

            // Add options to dropdown
            dropdown.AddOptions(optionsList);
        }

        /// Populates the dropdown with enum values, excluding any specified members
        public static void PopulateDropdownFromEnum<T>(this TMP_Dropdown dropdown, params T[] excludedValues) where T : Enum
        {
            dropdown.ClearOptions();

            // Get all enum values, then exclude the ones passed in
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>()
                .Where(value => excludedValues == null || !excludedValues.Contains(value));

            // Convert to string options with formatting
            var optionsList = enumValues
                .Select(value => value.ToString().ToReadableFromEnum())
                .ToList();

            // ----------------------------------------- POPULATE DROPDOWN -------------------------------------------------
            if (optionsList.Count == 0)
            {
                Debug.LogError($"{nameof(PopulateDropdownFromEnum)} No valid enum values available after exclusions.");
            }

            dropdown.AddOptions(optionsList);
        }

        public static Vector2Int ToVector2Int(this (int, int) tuple)
        {
            return new Vector2Int(tuple.Item1, tuple.Item2);
        }

        public static ValueTuple<int, int> ToTuple(this Vector2Int vector)
        {
            return new ValueTuple<int, int>(vector.x, vector.y);
        }

        /// Convert enum name like "A_SORT_OF_ENUM" into "A sort of enum"
        public static string ToReadableFromEnum(this string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
            {
                Debug.LogError("[EnumStringConverter] Enum name is null or empty, cannot convert.");
                return string.Empty;
            }

            // Split on underscores, capitalize the first letter, lowercase the rest
            var words = enumName.Split('_')
                .Select(word => string.IsNullOrEmpty(word)
                    ? string.Empty
                    : char.ToUpper(word[0]) + word.Substring(1).ToLower());

            // Join words with spaces
            return string.Join(" ", words);
        }
        
        /// Returns the center point of a room
        public static Vector2Int GetCenter(this RectInt room)
        {
            return new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);
        }
    }
}