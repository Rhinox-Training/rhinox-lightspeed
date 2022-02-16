using UnityEngine;
using UnityEditor;
using System.IO;
using Rhinox.GUIUtils.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using MeshUtility = UnityEditor.MeshUtility;

#if UNITY_2019_2_OR_NEWER && PROBUILDER
using UnityEngine.ProBuilder;
#elif PROBUILDER
// Downgrade to probuilder 3.x if you get this error; No way to check :(
using ProBuilder.Core;
#endif

namespace Rhinox.Utilities.Editor
{
    [CustomEditor(typeof(MeshFilter))]
    public class MeshFilterEditor :  UnityEditor.Editor
    {
        private bool _showPivotChanger = false;
        private bool _onlyThisMesh = true;

        private Vector3 _pivot; //Pivot value -1..1, calculated from Mesh bounds
        private Vector3 _previousPivot; //Last used pivot
        private Mesh _mesh; //Mesh of the selected object
        private Collider _col; //Collider of the selected object

        private Mesh _originalMesh;
        private Vector3 _originalPivot;
        private Vector3 _originalPosition;

        private bool _pivotUnchanged; //Flag to decide when to instantiate a copy of the mesh

        private MeshFilter _targetObject;

        private bool _preservePosition = true;

        // MeshFilter[] TargetObjects;

        protected void OnEnable()
        {
            _targetObject = (MeshFilter) target;
            // TargetObjects = targets.OfType<MeshFilter>().ToArray();
        }

        public override void OnInspectorGUI()
        {
            using (new eUtility.HorizontalGroup())
            {
                base.OnInspectorGUI();

                serializedObject.Update();
                
                DrawMeshSaveOptions();
                
                serializedObject.ApplyModifiedProperties();
            }
            
            if (_targetObject.sharedMesh == null) return;

            if (!_showPivotChanger)
            {
                DrawEditOptions();
            }
            else
            {
                DrawPivotEditor();

                // Show mesh bounds after the button
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Bounds:", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(_mesh.bounds.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPivotEditor()
        {
            EditorGUILayout.BeginHorizontal();

            // Create a button to set the pivot to the center of the mesh
            if (GUILayout.Button("Center", SirenixGUIStyles.MiniButton))
            {
                if (_pivotUnchanged)
                {
                    if (_onlyThisMesh)
                    {
                        // deep copy mesh so you don't alter original
                        _mesh = Mesh.Instantiate(_targetObject.sharedMesh) as Mesh; //make a deep copy
                        _targetObject.mesh = _mesh;
                    }
                    else _mesh = _targetObject.sharedMesh;
                }

                _pivotUnchanged = false;
                _pivot = Vector3.zero;
                UpdatePivot();
                _previousPivot = _pivot;
            }

            GUILayout.FlexibleSpace();

            _preservePosition = EditorGUILayout.ToggleLeft("Preserve Position", _preservePosition);

            if (GUILayout.Button("Undo", SirenixGUIStyles.MiniButtonLeft))
            {
                _previousPivot = _pivot;
                _pivot = _originalPivot;
                UpdatePivot();

                _mesh = _originalMesh;
                _targetObject.mesh = _mesh;

                _targetObject.transform.position = _originalPosition;
                UpdatePivotVector();
                _pivotUnchanged = true;
            }

            if (GUILayout.Button("Apply", SirenixGUIStyles.MiniButtonRight))
            {
                _mesh = null;
                _pivotUnchanged = false;
                _showPivotChanger = false;
            }

            EditorGUILayout.EndHorizontal();

            _pivot.x = EditorGUILayout.Slider("X", _pivot.x, -1.0f, 1.0f);
            _pivot.y = EditorGUILayout.Slider("Y", _pivot.y, -1.0f, 1.0f);
            _pivot.z = EditorGUILayout.Slider("Z", _pivot.z, -1.0f, 1.0f);

            // Detects user input on any of the three sliders
            if (_pivot != _previousPivot)
            {
                if (_pivotUnchanged)
                {
                    if (_onlyThisMesh)
                    {
                        // deep copy mesh so you don't alter original
                        _mesh = Mesh.Instantiate(_targetObject.sharedMesh) as Mesh; //make a deep copy
                        _targetObject.mesh = _mesh;
                    }
                    else _mesh = _targetObject.sharedMesh;
                }

                _pivotUnchanged = false;
                UpdatePivot();
                _previousPivot = _pivot;
            }
        }

        private void DrawEditOptions()
        {
            using (new eUtility.HorizontalGroup(disabled: true))
                EditorGUILayout.Vector3Field("Bounds", _targetObject.sharedMesh.bounds.extents);

            using (new eUtility.HorizontalGroup())
            {
                using (new eUtility.VerticalGroup(GUILayoutOptions.ExpandWidth(true)))
                {
                    GUILayout.Label("Edit Pivot", SirenixGUIStyles.BoldLabel, GUILayoutOptions.ExpandWidth());

                    GUIHelper.PushIndentLevel(EditorGUI.indentLevel + 1);
                    _preservePosition = EditorGUILayout.ToggleLeft("Preserve Position", _preservePosition, GUILayoutOptions.ExpandWidth(false));
                    GUIHelper.PopIndentLevel();
                }

                using (new eUtility.VerticalGroup())
                {
                    if (GUILayout.Button("Only This Mesh"))
                    {
                        Undo.RecordObject(_targetObject, "Change Pivot of " + _targetObject.gameObject.name);
                        _onlyThisMesh = true;
                        PrepareForPivotChanges();
                    }
                    else if (GUILayout.Button("On Root Mesh"))
                    {
                        Undo.RecordObject(_targetObject, "Change Pivot of " + _targetObject.gameObject.name);
                        Undo.RecordObject(_targetObject.sharedMesh, "Change Pivot of " + _targetObject.gameObject.name);
                        _onlyThisMesh = false;
                        PrepareForPivotChanges();
                    }
                    else _mesh = null;
                }
            }
        }

        private void DrawMeshSaveOptions()
        {
            if (_targetObject.sharedMesh == null) return;
            
#if PROBUILDER
#if UNITY_2019_2_OR_NEWER
            var hasProbuilder = _targetObject.GetComponent<ProBuilderMesh>() != null;
#else
            var hasProbuilder = _targetObject.GetComponent<pb_Object>() != null;
#endif

            if (hasProbuilder)
            {
                if (GUILayout.Button("Remove ProBuilder", GUILayoutOptions.ExpandWidth(false)))
                {
                    EditorApplication.ExecuteMenuItem("Tools/ProBuilder/Actions/Strip ProBuilder Scripts in Selection");
                }

                return;
            }
#endif
            
            var path = AssetDatabase.GetAssetPath(_targetObject.sharedMesh);

            if (!string.IsNullOrWhiteSpace(path)) return;
            
            if (GUILayout.Button("Save Mesh", GUILayoutOptions.ExpandWidth(false)))
            {
                var rootPath = Path.Combine(Application.dataPath, "_Models");
                var name = _targetObject.gameObject.name;
                name = name.Replace(" ", "_");
                path = EditorUtility.SaveFilePanel("Where to save?", rootPath, name, "asset");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    rootPath = Path.GetFullPath(Path.Combine(rootPath, "..", ".."));
                    path = Utility.GetRelativePath(path, rootPath);
                    AssetDatabase.CreateAsset(_targetObject.sharedMesh, path);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        void PrepareForPivotChanges()
        {
            _showPivotChanger = true;

            // keep some stuff to restore if needed
            _originalMesh = _targetObject.sharedMesh;
            _originalPosition = _targetObject.transform.position;

            // update some things
            _mesh = _targetObject.sharedMesh;
            _col = _targetObject.GetComponent<Collider>();
            UpdatePivotVector();
            _pivotUnchanged = true;

            _originalPivot = _pivot;
        }

        //Achieve the movement of the pivot by moving the transform position in the specified direction
        //and then moving all vertices of the mesh in the opposite direction back to where they were in world-space
        void UpdatePivot()
        {
            //Calculate difference in 3d position
            Vector3 diff = Vector3.Scale(_mesh.bounds.extents, _previousPivot - _pivot);
            //Move object position by taking localScale into account
            if (_preservePosition)
                _targetObject.transform.position = _targetObject.transform.TransformPoint(-diff);
            //Iterate over all vertices and move them in the opposite direction of the object position movement
            Vector3[] verts = _mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] += diff;
            }

            //Assign the vertex array back to the mesh
            _mesh.vertices = verts;
            //Recalculate bounds of the mesh, for the renderer's sake
            //The 'center' parameter of certain colliders needs to be adjusted
            //when the transform position is modified
            _mesh.RecalculateBounds();

            ShiftCollider(diff);
        }

        private void ShiftCollider(Vector3 movement)
        {
            if (!_col) return;
            
            switch (_col)
            {
                case BoxCollider BoxCollider:
                    BoxCollider.center += movement;
                    break;
                case CapsuleCollider capsuleCollider:
                    capsuleCollider.center += movement;
                    break;
                case SphereCollider sphereCollider:
                    sphereCollider.center += movement;
                    break;
                default:
                    Debug.LogWarning("Unimplemented Collider for shifting; Collider might not be accurate correct anymore.");
                    break;
            }
        }

        //Look at the object's transform position in comparison to the center of its mesh bounds
        //and calculate the pivot values for xyz
        void UpdatePivotVector()
        {
            Bounds b = _mesh.bounds;
            Vector3 offset = -1 * b.center;
            _pivot = _previousPivot =
                new Vector3(offset.x / b.extents.x, offset.y / b.extents.y, offset.z / b.extents.z);
        }
    }

    public static class MeshUtilityEditor
    {
        [MenuItem("CONTEXT/MeshFilter/Move Mesh To Child")]
        private static void MoveToChild (MenuCommand menuCommand)
        {
            MeshFilter mf = menuCommand.context as MeshFilter;
            Mesh m = mf.sharedMesh;

            var t = mf.transform;
            var child = t.Find("Mesh");
            if (!child)
            {
                child = t.Create("Mesh");
                child.gameObject.CopyObjectSettingsFrom(t.gameObject);
            }
            
            // Copy meshfilter (after meshrenderer due to requirecomponent)
            var newFilter = Undo.AddComponent<MeshFilter>(child.gameObject);

            newFilter.mesh = mf.sharedMesh;

            Undo.DestroyObjectImmediate(mf);
            
            // Copy MeshRenderer as well
            var renderer = t.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                var newRenderer = Undo.AddComponent<MeshRenderer>(child.gameObject);
                
                newRenderer.materials = renderer.sharedMaterials;
                newRenderer.additionalVertexStreams = renderer.additionalVertexStreams;
#if UNITY_2019_2_OR_NEWER
                newRenderer.receiveGI = renderer.receiveGI;
#endif
#if UNITY_2019_4_OR_NEWER // Introduced in _3 but only in the latter versions
                newRenderer.scaleInLightmap = renderer.scaleInLightmap;
                newRenderer.stitchLightmapSeams = renderer.stitchLightmapSeams;
#endif
                newRenderer.receiveShadows = renderer.receiveShadows;
                newRenderer.lightmapIndex = renderer.lightmapIndex;
                newRenderer.probeAnchor = renderer.probeAnchor;
                newRenderer.rendererPriority = renderer.rendererPriority;
                newRenderer.shadowCastingMode = renderer.shadowCastingMode;
                
                Undo.DestroyObjectImmediate(renderer);
            }
        }
        
        
	
    }

    
}
