using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace SamsBackpack.RenderFeatureStash
{
    [CustomEditor(typeof(TextureInfo))]
    public class TextureInfoEditor : Editor
    {
        private TextureInfo tex;

        //UI elements
        private VisualElement customProperties;
        private PropertyField clearColorField;
        private PropertyField graphicFormatField;
        private PropertyField screenRatioField;
        private PropertyField fixedSizeField;

        private SerializedProperty sourceProperty;
        private SerializedProperty clearProperty;
        private SerializedProperty customGraphicFormatProperty;
        private SerializedProperty sizeMode;

        public override VisualElement CreateInspectorGUI()
        {
            tex = target as TextureInfo;

            VisualElement root = new VisualElement();
            root.Add(Field(nameof(tex.source), out sourceProperty));

            customProperties = new VisualElement();
            root.Add(customProperties);

            customProperties.Add(Field(nameof(tex.enableRandomWrite)));
            customProperties.Add(Field(nameof(tex.clear), out clearProperty));
            customProperties.Add(Field(nameof(tex.clearColor), out clearColorField));
            customProperties.Add(Field(nameof(tex.wrapMode)));
            customProperties.Add(Field(nameof(tex.filterMode)));
            customProperties.Add(Field(nameof(tex.customGraphicFormat), out customGraphicFormatProperty));
            customProperties.Add(Field(nameof(tex.graphicFormat), out graphicFormatField));
            customProperties.Add(Field(nameof(tex.sizeMode), out sizeMode));
            customProperties.Add(Field(nameof(tex.screenRatio), out screenRatioField));
            customProperties.Add(Field(nameof(tex.fixedSize), out fixedSizeField));

            return root;
        }

        private void UpdateVisibilities()
        {
            SetVisibility(customProperties, sourceProperty.enumValueIndex == 0);
            SetVisibility(graphicFormatField, customGraphicFormatProperty.boolValue);
            SetVisibility(clearColorField, clearProperty.boolValue);
            SetVisibility(screenRatioField, sizeMode.enumValueIndex == (int)StashSizeMode.ScreenRatio);
            SetVisibility(fixedSizeField, sizeMode.enumValueIndex == (int)StashSizeMode.FixedSize);
        }

        private void SetVisibility(VisualElement element, bool visible)
        {
            if (element == null)
                return;
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private PropertyField Field(string name)
        {
            SerializedProperty property = serializedObject.FindProperty(name);
            return new PropertyField(property);
        }

        private PropertyField Field(string name, out PropertyField field)
        {
            SerializedProperty property = serializedObject.FindProperty(name);
            field = new PropertyField(property);
            field.RegisterValueChangeCallback((SerializedPropertyChangeEvent e) => UpdateVisibilities());
            return field;
        }

        private PropertyField Field(string name, out PropertyField field, out SerializedProperty property)
        {
            property = serializedObject.FindProperty(name);
            field = new PropertyField(property);
            field.RegisterValueChangeCallback((SerializedPropertyChangeEvent e) => UpdateVisibilities());
            return field;
        }

        private PropertyField Field(string name, out SerializedProperty property)
        {
            property = serializedObject.FindProperty(name);
            PropertyField field = new PropertyField(property);
            field.RegisterValueChangeCallback((SerializedPropertyChangeEvent e) => UpdateVisibilities());
            return field;
        }

    }
}
