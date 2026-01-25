using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public static class SqlTableParser
    {
        public static SqlTable Parse(string sql)
        {
            var table = new SqlTable();
            var lines = sql.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var tableNameMatch = Regex.Match(sql, @"CREATE\s+TABLE\s+(\w+)", RegexOptions.IgnoreCase);
            if (tableNameMatch.Success)
                table.Name = tableNameMatch.Groups[1].Value.Trim();

            bool inColumns = false;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                {
                    inColumns = true;
                    continue;
                }
                if (line.StartsWith(")")) break;
                if (!inColumns || line == "" || line.StartsWith("--")) continue;

                // Remove trailing comma
                if (line.EndsWith(",")) line = line.Substring(0, line.Length - 1);

                // Computed column
                if (line.Contains(" AS ", StringComparison.OrdinalIgnoreCase))
                {
                    var asIdx = line.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
                    var name = line.Substring(0, asIdx).Trim();
                    var expr = line.Substring(asIdx + 4).Trim();
                    table.Columns.Add(new SqlColumn { Name = name, IsComputed = true, ComputedExpression = expr });
                    continue;
                }

                // Parse: Name Type [NULL|NOT NULL] [PRIMARY KEY] [IDENTITY] [DEFAULT ...]
                var parts = Regex.Split(line, @"\s+");
                if (parts.Length < 2) continue;

                string targetPattern = @"(?i)\b\w+\s*\(\s*\d+";

                if (Regex.IsMatch(parts[1], targetPattern) && !parts[1].Contains(")"))
                {
                    int i = 2;
                    while (!parts[1].Contains(")") && parts.Length > 2)
                    {
                        parts[1] += parts[i];

                        i++;
                    }
                }

                var col = new SqlColumn { Name = parts[0], SqlType = parts[1] };
                var rest = line.Substring(parts[0].Length + parts[1].Length).ToUpper();
                col.IsNullable = !rest.Contains("NOT NULL") && !rest.Contains("DEFAULT");
                col.IsPrimaryKey = rest.Contains("PRIMARY KEY");
                col.IsIdentity = rest.Contains("IDENTITY");
                col.IsComputed = false;
                var defMatch = Regex.Match(rest, @"DEFAULT\s+([^,\s]+)");
                if (defMatch.Success) col.DefaultValue = defMatch.Groups[1].Value;
                table.Columns.Add(col);
            }
            return table;
        }
    }
}
