using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public class BllCodeBuilder
    {
        public static string GenerateBll(SqlTable table)
        {
            var sb = new StringBuilder();
            string tbl = table.Name;
            var cols = table.Columns;
            var pk = cols.Find(c => c.IsPrimaryKey) ?? cols[0];
            string objName = pk.Name.Substring(0, pk.Name.Length - 2);
            var nonComputedCols = cols.FindAll(c => !c.IsComputed).Where(c => c.Name.ToLower() != "isdeleted").ToList();
            var insertCols = nonComputedCols.FindAll(c => !c.IsIdentity && c.Name.ToLower() != "isdeleted" && c.Name.ToLower() != "updatedbyuserid" && c.Name.ToLower() != "updateddate" && c.Name.ToLower() != "createddate");
            var updateCols = nonComputedCols.FindAll(c => !c.IsPrimaryKey && !c.IsIdentity && c.Name.ToLower() != "isdeleted" && c.Name.ToLower() != "createdbyuserid" && c.Name.ToLower() != "createddate" && c.Name.ToLower() != "updateddate");

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("namespace RMS_Business\n{");
            sb.AppendLine($"    public class cls{objName}\n    {{");
            sb.AppendLine("        public enum enMode { AddNew = 0, Update = 1 };");
            sb.AppendLine("        public enMode Mode = enMode.AddNew;");

            // Properties
            foreach (var c in nonComputedCols)
            {
                string csType = SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable);
                sb.AppendLine($"        public {csType} {c.Name} {{ get; set; }}");
            }

            // Constructor
            sb.AppendLine($"        public cls{objName}()");
            sb.AppendLine("        {");
            foreach (var c in nonComputedCols)
            {
                string csType = SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable);
                string defaultVal = csType.EndsWith("?") || csType == "string" ? "null" : (csType == "string" ? "string.Empty" : "-1");
                if (csType == "string") defaultVal = "string.Empty";
                else if (csType.EndsWith("?")) defaultVal = "null";
                else if (csType == "bool") defaultVal = "false";
                else if (csType == "DateTime") defaultVal = "DateTime.MinValue";
                else defaultVal = "-1";
                sb.AppendLine($"            {c.Name} = {defaultVal};");
            }
            sb.AppendLine("            Mode = enMode.AddNew;");
            sb.AppendLine("        }");

            // Save method
            sb.AppendLine("        public bool Save()");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (Mode)");
            sb.AppendLine("            {");
            sb.AppendLine("                case enMode.AddNew:");
            sb.AppendLine($"                    var newID = cls{objName}Data.AddNew{objName}({string.Join(", ", insertCols.ConvertAll(c => c.Name))});");
            sb.AppendLine($"                    if (newID != -1) {{ {pk.Name} = newID; Mode = enMode.Update; return true; }}");
            sb.AppendLine("                    else return false;");
            sb.AppendLine("                case enMode.Update:");
            sb.AppendLine($"                    return cls{objName}Data.Update{objName}({pk.Name}, {string.Join(", ", updateCols.ConvertAll(c => c.Name))});");
            sb.AppendLine("            }");
            sb.AppendLine("            return false;");
            sb.AppendLine("        }");

            // Static Find
            sb.AppendLine($"        public static cls{objName}? Find({SqlTypeMapper.ToCSharpType(pk.SqlType, pk.IsNullable)} {pk.Name})");
            sb.AppendLine("        {");
            foreach (var c in nonComputedCols)
            {
                if (c.Name == pk.Name) continue;
                string csType = SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable);
                string defaultVal = csType.EndsWith("?") || csType == "string" ? "null" : (csType == "string" ? "string.Empty" : "-1");
                if (csType == "string") defaultVal = "string.Empty";
                else if (csType.EndsWith("?")) defaultVal = "null";
                else if (csType == "bool") defaultVal = "false";
                else if (csType == "DateTime") defaultVal = "DateTime.MinValue";
                else defaultVal = "-1";
                sb.AppendLine($"            {csType} {c.Name} = {defaultVal};");
            }
            sb.AppendLine($"            bool found = cls{objName}Data.Get{objName}ByID({pk.Name}, ref {string.Join(", ref ", nonComputedCols.FindAll(c => c.Name != pk.Name).ConvertAll(c => c.Name))});");
            sb.AppendLine("            if (found)");
            sb.AppendLine($"                return new cls{objName}() {{ {string.Join(", ", nonComputedCols.ConvertAll(c => c.Name + " = " + c.Name))}, Mode = enMode.Update }};");
            sb.AppendLine("            else return null;");
            sb.AppendLine("        }");

            // Static Delete
            sb.AppendLine($"        public static bool Delete{objName}({SqlTypeMapper.ToCSharpType(pk.SqlType, pk.IsNullable)} {pk.Name}{(cols.Any(c => c.Name.ToLower() == "updatedbyuserid") ? ", int? UpdatedByUserID = null" : "")})");
            sb.AppendLine("        {");
            sb.AppendLine($"            return cls{objName}Data.Delete{objName}({pk.Name} {(cols.Any(c => c.Name.ToLower() == "updatedbyuserid") ? ", UpdatedByUserID" : "")});");
            sb.AppendLine("        }");

            // Static GetAll
            sb.AppendLine($"        public static DataTable GetAll{objName}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return cls{objName}Data.GetAll{objName}();");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
