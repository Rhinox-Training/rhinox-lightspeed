using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Sirenix.Utilities;

namespace Rhinox.Utilities.Editor
{
	public static class ScriptableObjectUtility
	{
		private static MethodInfo _createAssetMethod;
		public static ScriptableObject CreateAsset(Type assetType, string directory, string name)
		{
			if (_createAssetMethod == null)
				_createAssetMethod = typeof(ScriptableObjectUtility)
					.GetMethods(Flags.StaticPublic)
					.Where(x => x.Name == nameof(CreateAsset))
					.FirstOrDefault(x => x.HasParamaters(new[] {typeof(string), typeof(string)}));


			if (!assetType.InheritsFrom<ScriptableObject>())
			{
				BetterLog.Error<UtilityLogger>("CreateAsset called with invalid type.");
				return null;
			}

			var createMethod = _createAssetMethod.MakeGenericMethod(assetType);
			
			var result = createMethod.Invoke(null, new object[] { directory, name });
			
			return result as ScriptableObject;
		}
		
		/// <summary>
		//	This makes it easy to create, name and place unique new ScriptableObject asset files.
		/// </summary>
		public static T CreateAsset<T>(string directory, string name) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T>();

			if (string.IsNullOrEmpty(directory))
				directory = "Assets";
			
			eUtility.CreateAssetsDirectory(directory);

			if (string.IsNullOrEmpty(name))
				name = $"New {typeof(T)}.asset";

			if (!name.EndsWith(".asset"))
				name += ".asset";
			
			var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, name));

			AssetDatabase.CreateAsset(asset, path);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// EditorUtility.FocusProjectWindow ();
			// Selection.activeObject = asset;

			return asset;
		}
		
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T>();

			if (string.IsNullOrEmpty(path))
				path = Path.Combine("Assets", $"New {typeof(T)}.asset");
			
			eUtility.CreateAssetsDirectory(Path.GetDirectoryName(path));

			path = AssetDatabase.GenerateUniqueAssetPath(path);

			AssetDatabase.CreateAsset(asset, path);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// EditorUtility.FocusProjectWindow ();
			// Selection.activeObject = asset;

			return asset;
		}
	}
}