#if TEXTMESHPRO
using TMPro;

namespace Rhinox.Lightspeed
{
    public static class TMPExtensions
    {
        public static string GetSelected(this TMP_Dropdown dropdown)
        {
            return dropdown.GetCurrentOption()?.text;
        }

        public static TMP_Dropdown.OptionData GetCurrentOption(this TMP_Dropdown dropdown)
        {
            return dropdown.value != -1 ? dropdown.options[dropdown.value] : null;
        }
        
        public static void RemoveTrailingChar(this TMP_InputField field)
        {
            if (string.IsNullOrEmpty(field.text))
                return;
            
            if (field.text.Length > 0) 
                field.text = field.text.Remove(field.text.Length - 1);
        }
    }
}
#endif