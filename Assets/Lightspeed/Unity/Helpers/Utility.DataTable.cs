using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static char GetCsvSeparator()
        {
            var numSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            char separator;
            if (numSeparator == ",") separator = ';';
            else if (numSeparator == ".") separator = ',';
            else separator = ';';
            return separator;
        }

        public static IEnumerable<string[]> ReadCsv(string[] lines) => ReadCsv(lines, GetCsvSeparator());

        public static IEnumerable<string[]> ReadCsv(string[] lines, char separator)
        {
            foreach (var line in lines)
            {
                var cells = line.Split(separator).Select(x => Regex.Replace(x, "^\"(.*)\"$", e => e.Groups[1].Value)).ToArray();
                yield return cells;
            }
        }
        
        public static DataTable ReadCsvTable(string[] lines)
        {
            return ReadCsvTable(lines, GetCsvSeparator());
        }

        public static DataTable ReadCsvTable(string[] lines, char separator)
        {
            var cells2D = ReadCsv(lines).ToArray();
            if (!cells2D.IsRectangular())
                return null;

            DataTable dt = new DataTable();
            foreach (var headerCell in cells2D[0])
                dt.Columns.Add(headerCell);

            string[] row = new string[cells2D[0].Length];
            for (int i = 1; i < cells2D.Length; ++i)
            {
                for (int j = 0; j < cells2D[i].Length; ++j)
                    row[j] = cells2D[i][j];
                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}