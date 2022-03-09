using Rhinox.Lightspeed.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Rhinox.Lightspeed.Editor
{
    [CustomPropertyDrawer(typeof(SerializableTupleCollection), true)]
    public class SerializableTupleCollectionDrawer : PropertyDrawer
    {
        private ReorderableList _list;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_list == null)
            {
                var listProp = property.FindPropertyRelative("list");
                _list = new ReorderableList(property.serializedObject, listProp, true, false, true, true);
                _list.drawElementCallback = DrawListItems;
            }

            var firstLine = position;
            firstLine.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(firstLine, property, label);

            if (property.isExpanded)
            {
                position.y += firstLine.height;

                var prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;

                _list.DoList(position);

                EditorGUIUtility.labelWidth = prevLabelWidth;
            }
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index); // The element in the list

            var keyProp = element.FindPropertyRelative("Item1");
            var contents = new GUIContent[] {GUIContent.none, new GUIContent(" => ")};

            EditorGUI.MultiPropertyField(rect, contents, keyProp,
                EditorGUI.BeginProperty(rect, new GUIContent($"Element {index}"), element));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                var listProp = property.FindPropertyRelative("list");
                if (listProp.arraySize < 2)
                    return EditorGUIUtility.singleLineHeight + 42f;
                else
                    return EditorGUIUtility.singleLineHeight + 21f * (listProp.arraySize + 1);
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }
}