using System.Linq;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static string GetCommandLineArg(string name, string defaultValue = null)
        {
            var args = System.Environment.GetCommandLineArgs().ToList();
            int index = args.IndexOf(name);
            if (index >= 0 && index < args.Count - 1)
            {
                return args[index + 1];
            }
            return defaultValue;
        }
    }
}