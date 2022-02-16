using System.Linq;
using UnityEditor;
using Rhinox.Lightspeed;

namespace Rhinox.Utilities.Editor
{
    public static class NumberingHelper
    {
        private const string AlphabeticNumberingHeader = "Rhinox/Update Alphabetic Numbering";

        [MenuItem(AlphabeticNumberingHeader, priority = 10000)]
        private static void UpdateAlphabeticNumbering()
        {
            int n = -1;
            const string UNDO_KEY = "Update Alphabetical numbering";

            var objs = Selection.gameObjects 
                // TODO actual order over multiple Parents?
                .OrderBy(x => x.transform.GetSiblingIndex())
                .ToArray();

            for (int i = 0; i < objs.Length; ++i)
            {
                var go = objs[i];
                ++n;
                
                var numberings = Utility.FindAlphabetNumbering(go.name);

                if (numberings.Length >= 1)
                {
                    var grp = numberings[0];
                    if (n == 0)
                    {
                        n = Utility.AlphabetToNum(grp.Value);
                        continue;
                    }
                    var alphaNum = Utility.NumToAlphabet(n);
                    Undo.RegisterCompleteObjectUndo(go, UNDO_KEY);
                    go.name = go.name.Replace(grp.Index, grp.Length, alphaNum);
                }
                else
                {
                    ++n;
                    Undo.RegisterCompleteObjectUndo(go, UNDO_KEY);
                    go.name += " " + Utility.NumToAlphabet(n);
                }
            }
        }

        [MenuItem(AlphabeticNumberingHeader, validate = true)]
        private static bool HasMultiSelection()
        {
            return Selection.gameObjects.Length > 1;
        }
    }
}