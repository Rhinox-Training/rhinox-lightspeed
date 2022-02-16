using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Utilities.Editor
{
    internal static class ContextMenuEnhancements
    {
        private const string _groupOpName = "GameObject/Group Object(s) %g";
        
        [MenuItem(_groupOpName, false, 10)]
        public static void GroupOperation(MenuCommand menuCommand)
        {
            //Prevent executing multiple times when right-clicking.
            if (Selection.objects.Length > 1)
            {
                if (menuCommand.context != null && menuCommand.context != Selection.objects[0])
                    return;
            }
            Selection.transforms.GroupTransforms(recordUndo: true);
        }
        
        [MenuItem(_groupOpName, true)]
        private static bool ValidateGroupOperation()
        {
            return Selection.transforms.Length != 0;
        }

        [MenuItem("CONTEXT/BoxCollider/Set Collider Bounds")]
        private static void SetColliderBounds(MenuCommand command)
        {
            BoxCollider coll = command.context as BoxCollider;

            // We want a list of colliders without the current collider
            // otherwise if it envelops all others, it will always just return itself
            var list = new List<Collider>();
            coll.GetComponentsInChildren(list);
            list.Remove(coll);
            var colliders = list.ToArray();
            var bounds = coll.gameObject.GetObjectBounds(null, colliders);
            
            Undo.RegisterCompleteObjectUndo(coll, "Set Collider Bounds");
            coll.center = coll.transform.InverseTransformPoint(bounds.center);
            coll.size = coll.transform.InverseTransformVector(bounds.size);
        }
        
#if !UNITY_2020_1_OR_NEWER
        [MenuItem("CONTEXT/MonoBehaviour/Properties")]
        private static void OpenInInspector(MenuCommand command)
        {
            GUIHelper.OpenInspectorWindow(command.context);
        }
#endif
    }
}