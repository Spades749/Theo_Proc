using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VTools.Utility
{
    /// <summary>
    /// Provides a collection of utility methods for common Unity operations including sprite creation,
    /// text rendering, mouse position handling, vector math, and UI/World coordinate conversions.
    /// </summary>
    public static class UtilityMethods
    {
        // ------------------------------------ SPRITE CREATION ------------------------------------

        /// <summary>
        /// Creates a sprite GameObject in world space without a parent transform.
        /// </summary>
        /// <param name="name">The name of the GameObject to create.</param>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="position">The world position of the sprite.</param>
        /// <param name="localScale">The scale of the sprite.</param>
        /// <param name="sortingOrder">The sorting order for the sprite renderer.</param>
        /// <param name="color">The color tint of the sprite.</param>
        /// <returns>The created GameObject with a SpriteRenderer component.</returns>
        public static GameObject CreateWorldSprite(string name, Sprite sprite, Vector3 position, Vector3 localScale, int sortingOrder, Color color)
        {
            return CreateWorldSprite(null, name, sprite, position, localScale, sortingOrder, color);
        }

        /// <summary>
        /// Creates a sprite GameObject in world space with an optional parent transform.
        /// </summary>
        /// <param name="parent">The parent transform for the sprite (can be null).</param>
        /// <param name="name">The name of the GameObject to create.</param>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="localPosition">The local position relative to the parent (or world position if no parent).</param>
        /// <param name="localScale">The scale of the sprite.</param>
        /// <param name="sortingOrder">The sorting order for the sprite renderer.</param>
        /// <param name="color">The color tint of the sprite.</param>
        /// <returns>The created GameObject with a SpriteRenderer component.</returns>
        public static GameObject CreateWorldSprite(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localScale = localScale;
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = color;
            return gameObject;
        }

        // ------------------------------------ TEXT CREATION ------------------------------------

        /// <summary>
        /// Creates a TextMesh GameObject in world space with optional parameters.
        /// </summary>
        /// <param name="text">The text content to display.</param>
        /// <param name="parent">The parent transform (null for no parent).</param>
        /// <param name="localPosition">The local position relative to the parent.</param>
        /// <param name="fontSize">The font size of the text.</param>
        /// <param name="color">The text color (defaults to white if null).</param>
        /// <param name="textAnchor">The anchor point for the text.</param>
        /// <param name="textAlignment">The alignment of the text.</param>
        /// <returns>The TextMesh component of the created GameObject.</returns>
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null,
            TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment);
        }

        /// <summary>
        /// Creates a TextMesh GameObject in world space with all parameters explicitly defined.
        /// </summary>
        /// <param name="parent">The parent transform (null for no parent).</param>
        /// <param name="text">The text content to display.</param>
        /// <param name="localPosition">The local position relative to the parent.</param>
        /// <param name="fontSize">The font size of the text.</param>
        /// <param name="color">The text color.</param>
        /// <param name="textAnchor">The anchor point for the text.</param>
        /// <param name="textAlignment">The alignment of the text.</param>
        /// <returns>The TextMesh component of the created GameObject.</returns>
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = 0.2f;
            textMesh.color = color;
            return textMesh;
        }

        // ------------------------------------ MOUSE POSITION ------------------------------------

        /// <summary>
        /// Gets the mouse position in world space with Z coordinate set to 0.
        /// Uses the main camera and current mouse position.
        /// </summary>
        /// <returns>The mouse position in world coordinates with Z = 0.</returns>
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        /// <summary>
        /// Gets the mouse position in world space using the main camera.
        /// </summary>
        /// <returns>The mouse position in world coordinates.</returns>
        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }

        /// <summary>
        /// Gets the mouse position in world space using a specified camera.
        /// </summary>
        /// <param name="worldCamera">The camera to use for the conversion.</param>
        /// <returns>The mouse position in world coordinates.</returns>
        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }

        /// <summary>
        /// Converts a screen position to world space using a specified camera.
        /// </summary>
        /// <param name="screenPosition">The screen position to convert.</param>
        /// <param name="worldCamera">The camera to use for the conversion.</param>
        /// <returns>The position in world coordinates.</returns>
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        // ------------------------------------ UI DETECTION ------------------------------------

        /// <summary>
        /// Determines whether the mouse pointer is currently over a UI element.
        /// Useful for preventing world interactions when clicking on UI.
        /// </summary>
        /// <returns>True if the pointer is over a UI element, false otherwise.</returns>
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            PointerEventData pe = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, hits);
            return hits.Count > 0;
        }

        // ------------------------------------ VECTOR & ANGLE MATH ------------------------------------

        /// <summary>
        /// Converts an angle in degrees to a direction vector.
        /// 0 degrees points right (1, 0), 90 degrees points up (0, 1).
        /// </summary>
        /// <param name="angle">The angle in degrees (0-360).</param>
        /// <returns>A normalized direction vector.</returns>
        public static Vector3 GetVectorFromAngle(int angle)
        {
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        /// <summary>
        /// Converts a direction vector to an angle in degrees (0-360).
        /// Returns a float value for precise angle calculations.
        /// </summary>
        /// <param name="dir">The direction vector.</param>
        /// <returns>The angle in degrees as a float (0-360).</returns>
        public static float GetAngleFromVectorFloat(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            return n;
        }

        /// <summary>
        /// Converts a direction vector to an angle in degrees (0-360).
        /// Returns an integer value rounded to the nearest degree.
        /// </summary>
        /// <param name="dir">The direction vector.</param>
        /// <returns>The angle in degrees as an integer (0-360).</returns>
        public static int GetAngleFromVector(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            int angle = Mathf.RoundToInt(n);
            return angle;
        }

        /// <summary>
        /// Converts a direction vector to an angle in degrees (-180 to 180).
        /// Useful for signed angle calculations.
        /// </summary>
        /// <param name="dir">The direction vector.</param>
        /// <returns>The angle in degrees as an integer (-180 to 180).</returns>
        public static int GetAngleFromVector180(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            int angle = Mathf.RoundToInt(n);
            return angle;
        }

        /// <summary>
        /// Applies a rotation to a vector based on a rotation vector's angle.
        /// </summary>
        /// <param name="vec">The vector to rotate.</param>
        /// <param name="vecRotation">The rotation vector defining the rotation angle.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector3 ApplyRotationToVector(Vector3 vec, Vector3 vecRotation)
        {
            return ApplyRotationToVector(vec, GetAngleFromVectorFloat(vecRotation));
        }

        /// <summary>
        /// Applies a rotation to a vector based on an angle in degrees.
        /// </summary>
        /// <param name="vec">The vector to rotate.</param>
        /// <param name="angle">The rotation angle in degrees.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector3 ApplyRotationToVector(Vector3 vec, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vec;
        }

        // ------------------------------------ COORDINATE CONVERSION ------------------------------------

        /// <summary>
        /// Converts a world position to a UI position in the specified UI space.
        /// </summary>
        /// <param name="worldPosition">The position in world coordinates.</param>
        /// <param name="parent">The parent transform of the UI element.</param>
        /// <param name="uiCamera">The camera rendering the UI.</param>
        /// <param name="worldCamera">The camera that rendered the world position.</param>
        /// <returns>The position in UI local space.</returns>
        public static Vector2 GetWorldUIPosition(Vector3 worldPosition, Transform parent, Camera uiCamera, Camera worldCamera)
        {
            Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
            Vector3 uiCameraWorldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
            Vector3 localPos = parent.InverseTransformPoint(uiCameraWorldPosition);
            return new Vector2(localPos.x, localPos.y);
        }

        /// <summary>
        /// Converts the current mouse UI position to world space with Z set to 0.
        /// Uses the main camera.
        /// </summary>
        /// <returns>The world position with Z = 0.</returns>
        public static Vector3 GetWorldPositionFromUIZeroZ()
        {
            Vector3 vec = GetWorldPositionFromUI(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        /// <summary>
        /// Converts the current mouse UI position to world space using the main camera.
        /// </summary>
        /// <returns>The world position.</returns>
        public static Vector3 GetWorldPositionFromUI()
        {
            return GetWorldPositionFromUI(Input.mousePosition, Camera.main);
        }

        /// <summary>
        /// Converts the current mouse UI position to world space using a specified camera.
        /// </summary>
        /// <param name="worldCamera">The camera to use for the conversion.</param>
        /// <returns>The world position.</returns>
        public static Vector3 GetWorldPositionFromUI(Camera worldCamera)
        {
            return GetWorldPositionFromUI(Input.mousePosition, worldCamera);
        }

        /// <summary>
        /// Converts a screen position to world space using a specified camera.
        /// </summary>
        /// <param name="screenPosition">The screen position to convert.</param>
        /// <param name="worldCamera">The camera to use for the conversion.</param>
        /// <returns>The world position.</returns>
        public static Vector3 GetWorldPositionFromUI(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
    }
}