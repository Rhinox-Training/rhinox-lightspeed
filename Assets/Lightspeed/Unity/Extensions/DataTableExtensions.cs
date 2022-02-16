using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Rhinox.Utilities
{
    public static class DataTableExtensions
    {
        public static string ToCsv(this DataTable table)
        {
            // Only return Null if there is no structure.
            if (table.Columns.Count == 0)
                return null;
            
            StringBuilder result = new StringBuilder();
            
            char separator = GetCsvSeparator();
            
            table.Columns.AppendAsCsv(result, separator);
            result.Append(Environment.NewLine);

            foreach (DataRow row in table.Rows)
            {
                row.AppendAsCsv(result, separator);
                result.Append(Environment.NewLine);
            }

            return result.ToString();
        }

        private static void AppendAsCsv(this DataColumnCollection columns, StringBuilder result) => AppendAsCsv(columns, result, GetCsvSeparator());

        private static void AppendAsCsv(this DataColumnCollection columns, StringBuilder result, char separator)
        {
            for (int i = 0; i < columns.Count; ++i)
            {
                var col = columns[i];

                string name = col.ColumnName?.Replace("\"", "\"\"");
                result.Append($"\"{name}\"");
                
                if (i < columns.Count-1)
                    result.Append(separator);
            }
        }

        private static void AppendAsCsv(this DataRow row, StringBuilder result, char separator)
        {
            for (int i = 0; i < row.Table.Columns.Count; ++i)
            {
                var data = row[i]?.ToString().Replace("\"", "\"\"");
                
                result.Append($"\"{data}\"");
                
                if (i < row.Table.Columns.Count-1)
                    result.Append(separator);
            }
        }

        public static string ToCsv(this DataColumnCollection columns) => ToCsv(columns, GetCsvSeparator());
        
        public static string ToCsv(this DataColumnCollection columns, char separator)
        {
            if (columns.Count == 0)
                return null;
            
            StringBuilder result = new StringBuilder();

            AppendAsCsv(columns, result, separator);

            return result.ToString();
        }

        public static string ToCsv(this DataRow row) => ToCsv(row, GetCsvSeparator());
        
        public static string ToCsv(this DataRow row, char separator)
        {
            if (row.Table.Columns.Count == 0)
                return null;
            
            StringBuilder result = new StringBuilder();

            AppendAsCsv(row, result, separator);

            return result.ToString();
        }
        
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
            var cells = lines.Select(a => a.Split(separator));
            return (cells);
        }
    }
}