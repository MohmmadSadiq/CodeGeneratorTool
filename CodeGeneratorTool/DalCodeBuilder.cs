using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public class DalCodeBuilder
    {
        public static string GenerateDal(SqlTable table)
        {
            var sb = new StringBuilder();
            string tbl = table.Name;
            var cols = table.Columns;
            var pk = cols.Find(c => c.IsPrimaryKey) ?? cols[0];
            string objName = pk.Name.Substring(0, pk.Name.Length - 2);
            var nonIdentityCols = cols.FindAll(c => !c.IsIdentity && !c.IsComputed);
            var insertCols = nonIdentityCols.FindAll(c => !c.IsComputed && c.Name.ToLower() != "isdeleted" && c.Name.ToLower() != "updatedbyuserid" && c.Name.ToLower() != "updateddate" && c.Name.ToLower() != "createddate");
            var updateCols = nonIdentityCols.FindAll(c => !c.IsPrimaryKey && !c.IsComputed && c.Name.ToLower() != "isdeleted" && c.Name.ToLower() != "createdbyuserid" && c.Name.ToLower() != "createddate" && c.Name.ToLower() != "updateddate");
            var selectCols = cols.FindAll(c => !c.IsComputed && c.Name.ToLower() != "isdeleted");

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using Microsoft.Data.SqlClient;");
            sb.AppendLine();
            sb.AppendLine($"namespace RMS_DataAccess\n{{");
            sb.AppendLine($"    public class cls{objName}Data\n    {{");

            // GetByID
            sb.AppendLine($"        public static bool Get{objName}ByID({SqlTypeMapper.ToCSharpType(pk.SqlType, pk.IsNullable)} {pk.Name}, ref " + string.Join(", ref ", selectCols.FindAll(c => c.Name != pk.Name).ConvertAll(c => SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable) + " " + c.Name)) + ")");
            sb.AppendLine("        {");
            sb.AppendLine("            bool isFound = false;");
            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"sp{objName}_GetByID\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine($"                    command.Parameters.AddWithValue(\"@{pk.Name}\", {pk.Name});");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        connection.Open();");
            sb.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            sb.AppendLine("                        {");
            sb.AppendLine("                            if (reader.Read())");
            sb.AppendLine("                            {");
            sb.AppendLine("                                isFound = true;");
            foreach (var c in selectCols)
            {
                if (c.Name == pk.Name) continue;
                string csType = SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable);
                if (csType.EndsWith("?"))
                    sb.AppendLine($"                                {c.Name} = reader[\"{c.Name}\"] != DBNull.Value ? ({csType})reader[\"{c.Name}\"] : null;");
                else
                    sb.AppendLine($"                                {c.Name} = ({csType})reader[\"{c.Name}\"];");
            }
            sb.AppendLine("                            }");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (Exception)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        isFound = false;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return isFound;");
            sb.AppendLine("        }");

            // AddNew
            sb.AppendLine($"        public static int AddNew{objName}({string.Join(", ", insertCols.ConvertAll(c => SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable) + " " + c.Name))})");
            sb.AppendLine("        {");
            sb.AppendLine("            int newID = -1;");
            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"sp{objName}_AddNew\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            foreach (var c in insertCols)
            {

                // sb.AppendLine($"    command.Parameters.Add(\"@{c.Name}\", System.Data.SqlDbType.{c.SqlTypeEnum}).Value = (object?){c.Name} ?? DBNull.Value;");
                sb.AppendLine($"                    command.Parameters.Add(\"@{c.Name}\", System.Data.SqlDbType.{c.SqlTypeEnum}).Value = (object?){c.Name} ?? DBNull.Value;");

            }
            sb.AppendLine($"                    SqlParameter outputIdParam = new SqlParameter(\"@New{pk.Name}\", SqlDbType.Int) {{ Direction = ParameterDirection.Output }};");
            sb.AppendLine("                    command.Parameters.Add(outputIdParam);");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        connection.Open();");
            sb.AppendLine("                        command.ExecuteNonQuery();");
            sb.AppendLine("                        if (outputIdParam.Value != DBNull.Value)");
            sb.AppendLine("                            newID = (int)outputIdParam.Value;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (Exception)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        // Log error");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return newID;");
            sb.AppendLine("        }");

            // Update
            sb.AppendLine($"        public static bool Update{objName}({SqlTypeMapper.ToCSharpType(pk.SqlType, pk.IsNullable)} {pk.Name}, {string.Join(", ", updateCols.ConvertAll(c => SqlTypeMapper.ToCSharpType(c.SqlType, c.IsNullable) + " " + c.Name))})");
            sb.AppendLine("        {");
            sb.AppendLine("            int result = 0;");
            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"sp{objName}_Update\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine($"                    command.Parameters.AddWithValue(\"@{pk.Name}\", {pk.Name});");
            foreach (var c in updateCols)
                sb.AppendLine($"                    command.Parameters.Add(\"@{c.Name}\", System.Data.SqlDbType.{c.SqlTypeEnum}).Value = (object?){c.Name} ?? DBNull.Value;");
            sb.AppendLine("                    SqlParameter returnParameter = new SqlParameter() { Direction = ParameterDirection.ReturnValue };");
            sb.AppendLine("                    command.Parameters.Add(returnParameter);");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        connection.Open();");
            sb.AppendLine("                        command.ExecuteNonQuery();");
            sb.AppendLine("                        result = (int)returnParameter.Value;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (Exception)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        return false;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return result == 1;");
            sb.AppendLine("        }");

            // Delete
            sb.AppendLine($"        public static bool Delete{objName}({SqlTypeMapper.ToCSharpType(pk.SqlType, pk.IsNullable)} {pk.Name} {(cols.Any(c => c.Name.ToLower() == "updatedbyuserid") ? ",int? UpdatedByUserID" : "")})");
            sb.AppendLine("        {");
            sb.AppendLine("            int result = 0;");
            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"sp{objName}_Delete\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine($"                    command.Parameters.AddWithValue(\"@{pk.Name}\", {pk.Name});");
            if (cols.Any(c => c.Name.ToLower() == "updatedbyuserid"))
                sb.AppendLine("                    command.Parameters.AddWithValue(\"@UpdatedByUserID\", (object?)UpdatedByUserID ?? DBNull.Value);");
            sb.AppendLine("                    SqlParameter returnParameter = new SqlParameter() { Direction = ParameterDirection.ReturnValue };");
            sb.AppendLine("                    command.Parameters.Add(returnParameter);");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        connection.Open();");
            sb.AppendLine("                        command.ExecuteNonQuery();");
            sb.AppendLine("                        result = (int)returnParameter.Value;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (Exception)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        // Log error");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return result == 1;");
            sb.AppendLine("        }");

            // GetAll
            sb.AppendLine($"        public static DataTable GetAll{objName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            DataTable dt = new DataTable();");
            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"sp{objName}_GetAll\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        connection.Open();");
            sb.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            sb.AppendLine("                        {");
            sb.AppendLine("                            if (reader.HasRows)");
            sb.AppendLine("                                dt.Load(reader);");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (Exception)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        // Log error");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return dt;");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
