using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

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