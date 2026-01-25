using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public static class SqlTypeMapper
    {
        public static string ToCSharpType(string sqlType, bool isNullable)
        {
            string baseType = sqlType.ToUpper();
            if (baseType.Contains("NVARCHAR") || baseType.Contains("VARCHAR") || baseType.Contains("CHAR") || baseType.Contains("TEXT"))
                return "string" + (isNullable ? "?" : "");
            if (baseType.StartsWith("INT"))
                return isNullable ? "int?" : "int";
            if (baseType.StartsWith("BIGINT"))
                return isNullable ? "long?" : "long";
            if (baseType.StartsWith("SMALLINT"))
                return isNullable ? "short?" : "short";
            if (baseType.StartsWith("TINYINT"))
                return isNullable ? "byte?" : "byte";
            if (baseType.StartsWith("BIT"))
                return isNullable ? "bool?" : "bool";
            if (baseType.StartsWith("DECIMAL") || baseType.StartsWith("NUMERIC") || baseType.StartsWith("MONEY") || baseType.StartsWith("SMALLMONEY"))
                return isNullable ? "decimal?" : "decimal";
            if (baseType.StartsWith("FLOAT"))
                return isNullable ? "double?" : "double";
            if (baseType.StartsWith("REAL"))
                return isNullable ? "float?" : "float";
            if (baseType.StartsWith("DATE") || baseType.StartsWith("DATETIME") || baseType.StartsWith("SMALLDATETIME"))
                return isNullable ? "DateTime?" : "DateTime";
            if (baseType.StartsWith("UNIQUEIDENTIFIER"))
                return isNullable ? "Guid?" : "Guid";
            if (baseType.StartsWith("VARBINARY") || baseType.StartsWith("BINARY") || baseType.StartsWith("IMAGE"))
                return "byte[]";
            // Default fallback
            return "object";
        }
    }
}
