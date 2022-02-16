using System;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Rhinox.Utilities.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Transform), true)]
	public class TransformCompEditor : UnityEditor.Editor
	{
		private static class Properties
		{
			public const string PosName = "m_LocalPosition";
			public const string RotName = "m_LocalRotation";
			public const string ScaleName = "m_LocalScale";
			
			public static GUIContent WorldPositionLabel = new GUIContent("WP", "World Position");
			public static GUIContent PositionLabel = new GUIContent("P", "Position; Click to rest to 0,0,0");
			public static GUIContent RotationLabel = new GUIContent("R", "Rotation; Click to rest to 0,0,0");
			public static GUIContent ScaleLabel = new GUIContent("S", "Scale; Click to rest to 1,1,1");
			
			
			public static GUIContent CenterBtnLabel = new GUIContent("Center", "Move object to center of children without affecting children");
			public static GUIContent ResetBtnLabel = new GUIContent("Reset", "Reset scale without affecting children");
			public static GUIContent PingBtnLabel = new GUIContent("Ping", "Ping the GameObject in the hierarchy");
			public const string OpenLockedBtnTooltip = "Open a locked inspector with this object selected";

			public static GUIContent CopyBtnLabel = new GUIContent("C", "Copy");
			public static GUIContent PasteBtnLabel = new GUIContent("P", "Paste"); 
		}
		
		Vector3 worldPos;
		SerializedProperty mPos;
		SerializedProperty mRot;
		SerializedProperty mScale;

		private static Vector3? positionClipboard = null;
		private static Quaternion? rotationClipboard = null;
		private static Vector3? scaleClipboard = null;

		private const int _buttonHeight = 18;

		private bool _initialized;
		private Transform _target;
		
		void OnEnable()
		{
			Init();
		}

		private bool Init()
		{
			if (serializedObject.targetObject == null) return false;

			_target = (Transform) serializedObject.targetObject;

			worldPos = _target.position;
			mPos = serializedObject.FindProperty(Properties.PosName);
			mRot = serializedObject.FindProperty(Properties.RotName);
			mScale = serializedObject.FindProperty(Properties.ScaleName);

			return _initialized = true;
		}

		public override void OnInspectorGUI()
		{
			// if cannot initialize, draw the base gui
			if (!_initialized && !Init())
			{
				base.OnInspectorGUI();
			}
			
			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 10;

			serializedObject.Update();

			using (new eUtility.HorizontalGroup())
			{
				using (new eUtility.VerticalGroup())
				{
					DrawPosition();
					DrawRotation();
					DrawScale();
					
					EditorGUIUtility.labelWidth = 20;
					DrawWorldPosition();
				}
				
				// SirenixEditorGUI.VerticalLineSeparator();
				
				using (new eUtility.VerticalGroup(false, GUILayout.Width(25)))
				{
					if (GUILayout.Button(Properties.CenterBtnLabel, GUILayout.Height(_buttonHeight)))
						HandlePivotShift();
					
					DrawCopyPaste();
					
					if (GUILayout.Button(Properties.ResetBtnLabel, SirenixGUIStyles.Button, GUILayout.Height(_buttonHeight)))
						HandleScaleShift();

					DrawHierarchyHelpers();
				}
			}
			
			EditorGUIUtility.labelWidth = labelWidth;

			serializedObject.ApplyModifiedProperties();
		}

		private void HandleScaleShift()
		{
			foreach (Transform t in targets)
				t.ShiftScaleTo(Vector3.one, true);
		}

		private void HandlePivotShift()
		{
			if (Event.current.button == 0)
			{
				ShiftPivot(t => t.gameObject.GetObjectBounds().center);
				return;
			}
				
			var menu = new GenericMenu();
				
			var content = new GUIContent("Center on Bounds");
			menu.AddItem(content, false, () => ShiftPivot(t => t.gameObject.GetObjectBounds().center));
			menu.AddSeparator(string.Empty);
				
			content = new GUIContent("Center on Origin");
			menu.AddItem(content, false,  () => ShiftPivot(t => t.parent == null ? Vector3.zero : t.parent.position));

			menu.ShowAsContext();
		}

		private void ShiftPivot(Func<Transform, Vector3> targetGetter)
		{
			foreach (Transform t in targets)
			{
				var newCenter = targetGetter(t);
				t.ShiftPivotTo(newCenter, true);
			}
		}

		private static void ShowPropertyContextMenu(SerializedProperty property, GenericMenu menu = null)
		{
			var t = typeof(EditorGUI);
			var m = t.GetMethod("DoPropertyContextMenu", BindingFlags.NonPublic | BindingFlags.Static);
			m.Invoke(null, new object[] { property, null, menu });
		}
		
		private static void DrawResetButton(SerializedProperty prop, GUIContent label,
			GenericMenu.MenuFunction copy, bool canCopy,
			GenericMenu.MenuFunction paste, bool canPaste, 
			GenericMenu.MenuFunction reset,
			float width = 20f)
		{
			if (!GUILayout.Button(label, GUILayout.Width(20f))) return;
			
			if (Event.current.button == 0)
			{
				reset.Invoke();
				return;
			}
				
			var menu = new GenericMenu();
				
			var content = new GUIContent("Reset");
			menu.AddItem(content, false, reset);
			menu.AddSeparator(string.Empty);
				
			content = new GUIContent("Copy");
			if (canCopy) menu.AddItem(content, false, copy);
			else menu.AddDisabledItem(content);
				
			content = new GUIContent("Paste");
			if (canPaste) menu.AddItem(content, false, paste);
			else menu.AddDisabledItem(content);
				
			menu.AddSeparator(string.Empty);
				
			ShowPropertyContextMenu(prop, menu);
		}

		private void DrawPosition()
		{
			GUILayout.BeginHorizontal();
			DrawResetButton(mPos, Properties.PositionLabel, 
				() => SaveToClipboard(scale: false, rot: false), targets.Length == 1, 
				() => ApplyClipboard(scale: false, rot: false), positionClipboard.HasValue,
				() => Set(mPos, Vector3.zero));
			
			EditorGUILayout.PropertyField(mPos.FindPropertyRelative("x"));
			EditorGUILayout.PropertyField(mPos.FindPropertyRelative("y"));
			EditorGUILayout.PropertyField(mPos.FindPropertyRelative("z"));
			GUILayout.EndHorizontal();
		}

		private void Set(SerializedProperty prop, Vector3 val)
		{
			prop.serializedObject.Update();
			prop.vector3Value = val;
			prop.serializedObject.ApplyModifiedProperties();
			_target.hasChanged = true;
		}
		
		private void Set(SerializedProperty prop, Quaternion val)
		{
			prop.serializedObject.Update();
			prop.quaternionValue = val;
			prop.serializedObject.ApplyModifiedProperties();
			_target.hasChanged = true;
		}

		void DrawRotation()
		{
			GUILayout.BeginHorizontal();
			DrawResetButton(mRot, Properties.RotationLabel,
				() => SaveToClipboard(scale: false, pos: false), targets.Length == 1,
				() => ApplyClipboard(scale: false, pos: false), rotationClipboard.HasValue,
				() => Set(mRot, Quaternion.identity));

			Vector3 visible = _target.localEulerAngles;

			visible.x = WrapAngle(visible.x);
			visible.y = WrapAngle(visible.y);
			visible.z = WrapAngle(visible.z);

			Axis changed = CheckDifference(mRot);
			Axis altered = Axis.None;

			GUILayoutOption opt = GUILayout.MinWidth(30f);

			if (FloatField("X", ref visible.x, (changed & Axis.X) != 0, false, opt)) altered |= Axis.X;
			if (FloatField("Y", ref visible.y, (changed & Axis.Y) != 0, false, opt)) altered |= Axis.Y;
			if (FloatField("Z", ref visible.z, (changed & Axis.Z) != 0, false, opt)) altered |= Axis.Z;
			
			if (altered != Axis.None)
			{
				RegisterUndo("Change Rotation", serializedObject.targetObjects);

				foreach (Transform t in serializedObject.targetObjects)
				{
					Vector3 v = t.localEulerAngles;

					if ((altered & Axis.X) != 0) v.x = visible.x;
					if ((altered & Axis.Y) != 0) v.y = visible.y;
					if ((altered & Axis.Z) != 0) v.z = visible.z;

					t.localEulerAngles = v;
				}
			}

			GUILayout.EndHorizontal();
		}

		private void DrawScale()
		{
			GUILayout.BeginHorizontal();
			{ 
				DrawResetButton(mScale, Properties.ScaleLabel, 
					() => SaveToClipboard(pos: false, rot: false), targets.Length == 1, 
					() => ApplyClipboard(pos: false, rot: false), scaleClipboard.HasValue,
					() => Set(mScale, Vector3.one));

				EditorGUILayout.PropertyField(mScale.FindPropertyRelative("x"));
				EditorGUILayout.PropertyField(mScale.FindPropertyRelative("y"));
				EditorGUILayout.PropertyField(mScale.FindPropertyRelative("z"));
			}
			GUILayout.EndHorizontal();
		}

		private void DrawWorldPosition()
		{
			using (new eUtility.HorizontalGroup())
			{
				using (new eUtility.DisabledGroup(true))
				{
					EditorGUILayout.LabelField(Properties.WorldPositionLabel, GUILayoutOptions.Width(20));
					EditorGUILayout.Vector3Field(GUIContent.none, worldPos);
				}
			}
		}

		private void DrawCopyPaste()
		{
			using (new eUtility.HorizontalGroup())
			{
				if (GUILayout.Button(Properties.CopyBtnLabel, SirenixGUIStyles.ButtonLeft, GUILayout.Height(_buttonHeight)))
					SaveToClipboard();

				using (new eUtility.DisabledGroup(!positionClipboard.HasValue))
				{
					if (GUILayout.Button(Properties.PasteBtnLabel, SirenixGUIStyles.ButtonRight, GUILayout.Height(_buttonHeight)))
					{
						ApplyClipboard();
						GUI.FocusControl(null);
					}
				}
			}
		}

		private void DrawHierarchyHelpers()
		{
			using (new eUtility.HorizontalGroup())
			{
				var go = ((Transform) target).gameObject;
				using (new eUtility.DisabledGroup(!go.activeInHierarchy))
				{
					if (GUILayout.Button(Properties.PingBtnLabel, SirenixGUIStyles.ButtonLeft, GUILayout.Height(_buttonHeight)))
						EditorGUIUtility.PingObject(go);
				}
			
				if (SirenixEditorGUI.IconButton(EditorIcons.Pen, SirenixGUIStyles.ButtonRight, _buttonHeight, _buttonHeight, Properties.OpenLockedBtnTooltip))
					GUIHelper.OpenInspectorWindow(go);
			}
		}

		private void SaveToClipboard(bool pos = true, bool rot = true, bool scale = true)
		{
			if (pos) positionClipboard = _target.localPosition;
			else positionClipboard = null;
			
			if (rot) rotationClipboard = _target.localRotation;
			else rotationClipboard = null;
			
			if (scale) scaleClipboard = _target.localScale;
			else scaleClipboard = null;
		}

		private void ApplyClipboard(bool pos = true, bool rot = true, bool scale = true)
		{
			// If applying nothing, just return
			if (!(pos   && positionClipboard.HasValue) &&
			    !(rot   && rotationClipboard.HasValue) &&
			    !(scale && scaleClipboard.HasValue)) return;
			
			Undo.RecordObjects(targets, "Paste Clipboard Values");
			for (int i = 0; i < targets.Length; i++)
			{
				var t = ((Transform) targets[i]);
				if (pos   && positionClipboard.HasValue) t.localPosition = positionClipboard.Value;
				if (rot   && rotationClipboard.HasValue) t.localRotation = rotationClipboard.Value;
				if (scale && scaleClipboard.HasValue) t.localScale = scaleClipboard.Value;
			}
		}

		#region Rotation

		Axis CheckDifference(Transform t, Vector3 original)
		{
			Vector3 next = t.localEulerAngles;

			Axis axes = Axis.None;

			if (Differs(next.x, original.x)) axes |= Axis.X;
			if (Differs(next.y, original.y)) axes |= Axis.Y;
			if (Differs(next.z, original.z)) axes |= Axis.Z;

			return axes;
		}

		Axis CheckDifference(SerializedProperty property)
		{
			Axis axes = Axis.None;

			if (property.hasMultipleDifferentValues)
			{
				Vector3 original = property.quaternionValue.eulerAngles;

				foreach (Transform t in serializedObject.targetObjects)
				{
					axes |= CheckDifference(t, original);
					if (axes == Axis.XYZ) break;
				}
			}

			return axes;
		}

		/// <summary>
		/// Draw an editable float field.
		/// </summary>
		/// <param name="hidden">Whether to replace the value with a dash</param>
		/// <param name="greyedOut">Whether the value should be greyed out or not</param>

		static bool FloatField(string name, ref float value, bool hidden, bool greyedOut, GUILayoutOption opt)
		{
			float newValue = value;
			GUI.changed = false;

			if (!hidden)
			{
				if (greyedOut)
				{
					GUI.color = new Color(0.7f, 0.7f, 0.7f);
					newValue = EditorGUILayout.FloatField(name, newValue, opt);
					GUI.color = Color.white;
				}
				else
				{
					newValue = EditorGUILayout.FloatField(name, newValue, opt);
				}
			}
			else if (greyedOut)
			{
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
				GUI.color = Color.white;
			}
			else
			{
				float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
			}

			if (GUI.changed && Differs(newValue, value))
			{
				value = newValue;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Because Mathf.Approximately is too sensitive.
		/// </summary>

		static bool Differs(float a, float b)
		{
			return Mathf.Abs(a - b) > 0.0001f;
		}
		
		static public void RegisterUndo(string name, params Object[] objects)
		{
			if (objects == null || objects.Length <= 0) return;
			
			Undo.RecordObjects(objects, name);

			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				EditorUtility.SetDirty(obj);
			}
		}

		static public float WrapAngle(float angle)
		{
			while (angle > 180f) angle -= 360f;
			while (angle < -180f) angle += 360f;
			return angle;
		}

		#endregion
		
		/// <summary>
		/// This is used through reflection by Unity to 'Focus' on an object
		/// Default focus in unity is scuft; so override it using the bounds
		/// This method defines whether this component defines valid bounds and can override the focus bounds
		/// </summary>
		public bool HasFrameBounds()
		{
			var t = ((Transform) target);
			
			// If there are MeshRenderers or colliders in the children
			// Let unity handle it => return false
			if (t.gameObject.GetComponentInChildren<MeshRenderer>()) return false;
			if (t.gameObject.GetComponentInChildren<Collider>()) return false;
			
			// Override ParticleSystem behaviour
			if (t.gameObject.GetComponent<ParticleSystemRenderer>()) return true;
			
			// If there are MeshRenderers or colliders in the parent
			// We override it (see below) => return true
			if (t.gameObject.GetComponentInParent<MeshRenderer>()) return true;
			if (t.gameObject.GetComponentInParent<Collider>()) return true;

			return true;
		}

		/// <summary>
		/// Same as above, but the actual implementation of the bounds
		/// Hardly ever triggered so no point in caching
		/// </summary>
		public Bounds OnGetFrameBounds()
		{
			// assuming it will not get here if there is a child mesh, hence not calculating that

			var t = (Transform) target;

			var system = t.gameObject.GetComponent<ParticleSystemRenderer>();
			if (system)
				return system.bounds;
			
			var parent = t.parent;
			if (parent != null)
			{
				// Find it from parent down (aka in parent or siblings)
				var b = parent.gameObject.GetObjectBounds();
				if (!b.Equals(default))
					return b;
				
				// If not found there, try to find it above the parent
				var mesh = t.gameObject.GetComponentInParent<MeshRenderer>();

				if (mesh != null)
					return mesh.bounds;
			
				var collider = t.gameObject.GetComponentInParent<Collider>();

				if (collider != null)
					return collider.bounds;
			}
			
			// default small bounds
			return new Bounds(t.position, Vector3.one);
		}
	}
}