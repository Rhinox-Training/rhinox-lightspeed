using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

// MIT License
//
// Copyright (c) 2018 JohannesMP
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Modified by Rhinox (extracted new base class)

namespace Rhinox.Lightspeed
{
    /// <summary>
    /// The base of wrapper that provides the means to safely serialize Scene Asset References.
    /// </summary>
    [Serializable, HideReferenceObjectPicker, RefactoringOldNamespace("")]
    public class SceneReference : ISerializationCallbackReceiver
    {
        // =============================================
        // Properties
        // =============================================

#if UNITY_EDITOR
        [SerializeField] protected string sceneGuid;
        public virtual Object SceneAsset { get; protected set; }
        protected bool IsValidSceneAsset
        {
            get
            {
                if (!SceneAsset) return false;

                return SceneAsset is SceneAsset;
            }
        }
#endif

        // This should only ever be set during serialization/deserialization!
        [SerializeField] protected string scenePath = string.Empty;

        // Use this when you want to actually have the scene path
        public string ScenePath
        {
            get => GetScenePath();
            set => SetScenePath(value);
        }

        public int BuildIndex => SceneUtility.GetBuildIndexByScenePath(scenePath);

        // =============================================
        // Constructor(s)
        // =============================================

        public SceneReference()
        {
        }

        public SceneReference(string path)
        {
            SetScenePath(path);
        }

        public SceneReference(Scene scene) : this(scene.path)
        {
        }

#if UNITY_EDITOR
        public SceneReference(SceneAsset scene)
        {
            ScenePath = AssetDatabase.GetAssetPath(scene);
        }

        // =============================================
        // Methods
        // =============================================

        protected string GetScenePathFromAsset()
        {
            return SceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(SceneAsset);
        }

        protected string GetGuidFromAssetPath()
        {
            return string.IsNullOrEmpty(scenePath) ? null : AssetDatabase.AssetPathToGUID(scenePath);
        }

        protected SceneAsset GetSceneAssetFromPath()
        {
            return string.IsNullOrEmpty(scenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }
#endif

        protected virtual string GetScenePath()
        {
#if UNITY_EDITOR
            // In editor we always use the asset's path
            return GetScenePathFromAsset();
#else
        // At runtime we rely on the stored path value which we assume was serialized correctly at build time.
        // See OnBeforeSerialize and OnAfterDeserialize
        return scenePath;
#endif
        }

        protected void SetScenePath(string path)
        {
            scenePath = path;
#if UNITY_EDITOR
            sceneGuid = GetGuidFromAssetPath();
            SceneAsset = GetSceneAssetFromPath();
#endif

        }

        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference.ScenePath;
        }


        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        protected virtual void HandleBeforeSerialize()
        {
            if (!string.IsNullOrEmpty(scenePath))
                sceneGuid = GetGuidFromAssetPath();
        }

        protected virtual void HandleAfterDeserialize()
        {
#if UNITY_EDITOR
            // Unsubscribe so we do not get spammed
            EditorApplication.update -= HandleAfterDeserialize;
#endif
            // No data to recover from
            if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneGuid))
                return;

            // Try to find asset from ScenePath
            if (!string.IsNullOrEmpty(scenePath))
                SceneAsset = GetSceneAssetFromPath();

            // No asset found, path was invalid. Try to recover it from the GUID
            if (!SceneAsset)
            {
                scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                SceneAsset = GetSceneAssetFromPath();
            }

            // Still no asset found, path & guid were invalid
            if (!SceneAsset)
            {
                scenePath = string.Empty;
                sceneGuid = string.Empty;
            }

        }
#endif

        protected bool Equals(SceneReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return scenePath == other.scenePath &&
#if UNITY_EDITOR
                   Equals(SceneAsset, other.SceneAsset) && sceneGuid == other.sceneGuid;
#else
               true;
#endif
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneReference)obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            hashCode = (hashCode * 397) ^ (scenePath != null ? scenePath.GetHashCode() : 0);
#if UNITY_EDITOR
            hashCode = (hashCode * 397) ^ (sceneGuid != null ? sceneGuid.GetHashCode() : 0);
#endif
            return hashCode;
        }
    }
}