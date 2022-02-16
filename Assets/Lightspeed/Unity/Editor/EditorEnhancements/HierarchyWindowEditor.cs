using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils;
using Rhinox.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using RectExtensions = Sirenix.Utilities.RectExtensions;

/// <summary>
/// Hierarchy Window Group Header
/// http://diegogiacomelli.com.br/unitytips-hierarchy-window-group-header
/// </summary>
[InitializeOnLoad]
internal static class HierarchyWindowEditor
{
    static readonly LayerMask IgnoreLayer = Utility.GetMask("Default");
    static readonly float GameObjectIconWidth = 16;

    private static Texture2D _gradientTexture;
    private static GUIContent _editorOnlyContent;

    static HierarchyWindowEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowLayerInfo;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowGroupHeader;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyEditorOnlyHeader;
    }

    static void HierarchyWindowGroupHeader(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null || !gameObject.name.StartsWith("===", System.StringComparison.Ordinal))
            return;
        
        const float gray = .45f;
        selectionRect.height -= 2;
        selectionRect.y += 1;
        EditorGUI.DrawRect(selectionRect, new Color(gray, gray, gray));
        var text = gameObject.name.Replace("=", "").ToUpperInvariant();

        var reg = Regex.Split(text, @"^=+\s*(.*?)\s*[=\-\s]*$");
            
        EditorGUI.LabelField(selectionRect, reg.FirstOrDefault(), SirenixGUIStyles.BoldLabelCentered);
    }
    
    static void HierarchyEditorOnlyHeader(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null || !gameObject.CompareTag("EditorOnly"))
            return;

        if (_editorOnlyContent == null)
            _editorOnlyContent = new GUIContent(EditorIcons.AlertCircle.Active, "Marked as EDITOR ONLY");

        DrawOnGradient(selectionRect, _editorOnlyContent, new Color(.55f, .04f, .03f));
    }

    static void HierarchyWindowLayerInfo(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null || IgnoreLayer.HasLayer(gameObject.layer)) return;

        // this is assuming EditorStyles.Label is approximately the same style used by the hierarchy
        float maxWidth = GUIContentHelper.CalcMaxLabelWidth(gameObject.name);
        
        var content = GUIHelper.TempContent(LayerMask.LayerToName(gameObject.layer));
        var style = SirenixGUIStyles.RightAlignedGreyMiniLabel;
        float contentMin, contentMax;
        style.CalcMinMaxWidth(content, out contentMin, out contentMax);

        // not enough space to draw it
        if (maxWidth + contentMax + GameObjectIconWidth >= selectionRect.width)
            return;

        EditorGUI.LabelField(selectionRect, content, style);
    }
    
    private static void DrawOnGradient(Rect rect, GUIContent content, Color color)
    {
        // Create gradient
        if (_gradientTexture == null)
        {
            int width = 50, height = (int) EditorGUIUtility.singleLineHeight;
            Gradient gradient = Utility.MakeGradient(Color.white, Color.white, (0, 0), (.35f, .2f), (.6f, .4f), (1f, .7f));
            
            _gradientTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            _gradientTexture.alphaIsTransparency = true;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                    _gradientTexture.SetPixel(x, y, gradient.Evaluate(x / (width-1f)));
            }
            _gradientTexture.Apply();
        }
        
        var size = GUI.skin.label.CalcSize(content);
        // make size fit unto the available rect
        var contentWidth = (EditorGUIUtility.singleLineHeight / size.y) * size.x;

        rect = RectExtensions.AlignRight(rect, contentWidth + 10);

        GUIHelper.PushColor(color);
        GUI.DrawTexture(rect, _gradientTexture);
        GUI.Label(RectExtensions.AlignRight(rect, contentWidth), content);
        GUIHelper.PopColor();
    }
}