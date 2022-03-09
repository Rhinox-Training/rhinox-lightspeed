using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

#endif

// Author: JohannesMP (2018-08-12)
// MIT License
// A wrapper that provides the means to safely serialize Scene Asset References.
//
// Internally we serialize an Object to the SceneAsset which only exists at editor time.
// Any time the object is serialized, we store the path provided by this Asset (assuming it was valid).
//
// This means that, come build time, the string path of the scene asset is always already stored, which if 
// the scene was added to the build settings means it can be loaded.
//
// It is up to the user to ensure the scene exists in the build settings so it is loadable at runtime.
// To help with this, a custom PropertyDrawer displays the scene build settings state.
//
//  Known issues:
// - When reverting back to a prefab which has the asset stored as null, Unity will show the property 
// as modified despite having just reverted. This only happens on the fist time, and reverting again fix it. 
// Under the hood the state is still always valid and serialized correctly regardless.

/// <summary>
/// A wrapper that provides the means to safely serialize Scene Asset References.
/// Main difference with SceneReferenceData: Serializes an Object and is thus unfit for a data layer
/// Due to that it has an extra reference point (the Scene Object) and is slightly more reliable
/// Note: works on MonoBehaviours/Objects but not in a data layer
/// TODO: This does show up as an option when creating a 'SceneReferenceData' object which can cause confusion
/// </summary>

namespace Rhinox.Lightspeed
{
    [Serializable, RefactoringOldNamespace("")]
    public class SceneReference : SceneReferenceData
    {
#if UNITY_EDITOR
        // What we use in editor to select the scene
        [SerializeField] private Object sceneAsset;

        public override Object SceneAsset
        {
            get => sceneAsset;
            protected set => sceneAsset = value;
        }
#endif

        public SceneReference()
        {
        }

        public SceneReference(string path) : base(path)
        {
        }

        public SceneReference(Scene scene) : base(scene)
        {
        }


#if UNITY_EDITOR
        public SceneReference(SceneAsset scene) : base(scene)
        {
            sceneAsset = scene;
        }

        protected override void HandleBeforeSerialize()
        {
            // Asset is invalid but have Path to try and recover from
            if (IsValidSceneAsset == false && !string.IsNullOrEmpty(scenePath))
            {
                sceneAsset = GetSceneAssetFromPath();
                sceneGuid = GetGuidFromAssetPath();
                if (sceneAsset == null) scenePath = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precedence and overwrites Path
            else
            {
                scenePath = GetScenePathFromAsset();
                sceneGuid = GetGuidFromAssetPath();
            }
        }

        protected override void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (IsValidSceneAsset) return;

            // Try to recover the SceneAsset from the data
            base.HandleAfterDeserialize();

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}