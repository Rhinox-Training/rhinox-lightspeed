using TMPro;

namespace Rhinox.Lightspeed
{
    public static class TMPExtensions
    {
        public static string GetSelected(this TMP_Dropdown dropdown)
        {
            return dropdown.options[dropdown.value].text;
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