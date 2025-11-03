using UnityEditor;

using UnityEngine;

namespace SamsBackpack.RenderFeatureStash
{
    [CustomPropertyDrawer(typeof(StashInput))]
    public class StashInputDrawer : PropertyDrawer
    {
        const float objFieldWidth = 120;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect r = position;
            r.width -= objFieldWidth;

            EditorGUI.PropertyField(r, property.FindPropertyRelative("propertyName"), label);

            r.x += r.width + 10;
            r.width = objFieldWidth - 10;
            EditorGUI.PropertyField(r, property.FindPropertyRelative("textureInfo"), new GUIContent(""));
        }
    }
}