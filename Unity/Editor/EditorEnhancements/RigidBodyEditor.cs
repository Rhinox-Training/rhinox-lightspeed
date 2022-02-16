using UnityEditor;
using UnityEngine;

namespace Rhinox.Utilities.Editor
{
    [CustomEditor(typeof(Rigidbody))]
    public class RigidBodyEditor : UnityEditor.Editor
    {
        private Rigidbody _target;

        private void OnEnable()
        {
            _target = (Rigidbody) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
                return;

            if (FlexibleButton("Reset Forces"))
            {
                _target.ResetInertiaTensor();
            }
        }

        private bool FlexibleButton(string text)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool result = GUILayout.Button(text);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return result;
        }
    }
}