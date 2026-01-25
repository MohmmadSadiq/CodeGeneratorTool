using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public static class SqlCodeBuilder
    {
        public static string GenerateProcedures(SqlTable table)
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
            sb.AppendLine("USE [RMS];");
            sb.AppendLine("GO\n");
            // AddNew
            sb.AppendLine($"CREATE PROCEDURE sp{objName}_AddNew");
            foreach (var c in insertCols)
                sb.AppendLine($"    @{c.Name} {c.SqlType},");
            sb.AppendLine($"    @New{pk.Name} INT OUTPUT");
            sb.AppendLine("AS\nBEGIN");
            sb.AppendLine("    SET NOCOUNT ON;\n");
            sb.AppendLine($"    INSERT INTO {tbl} (");
            sb.AppendLine("        " + string.Join(", ", insertCols.ConvertAll(c => c.Name)));
            sb.AppendLine($"        {(cols.Any(c => c.Name.ToLower() == "createddate") ? ", CreatedDate" : "")} {(cols.Any(c => c.Name.ToLower() == "isdeleted") ? ", IsDeleted" : "")}");
            sb.AppendLine("    )\n    VALUES (");
            sb.AppendLine("        " + string.Join(", ", insertCols.ConvertAll(c => "@" + c.Name)));
            sb.AppendLine($"        {(cols.Any(c => c.Name.ToLower() == "createddate") ? ", GETDATE()" : "")} {(cols.Any(c => c.Name.ToLower() == "isdeleted") ? ", 0" : "")}");
            sb.AppendLine("    );\n");
            sb.AppendLine($"    SET @New{pk.Name} = SCOPE_IDENTITY();");
            sb.AppendLine("END\nGO\n");

            // GetAll
            sb.AppendLine($"CREATE PROCEDURE sp{objName}_GetAll AS\nBEGIN");
            sb.AppendLine("    SET NOCOUNT ON;\n");
            sb.AppendLine($"    SELECT {string.Join(", ", selectCols.ConvertAll(c => c.Name))} FROM {tbl} WHERE IsDeleted = 0;");
            sb.AppendLine("END\nGO\n");

            // GetByID
            sb.AppendLine($"CREATE PROCEDURE sp{objName}_GetByID");
            sb.AppendLine($"    @{pk.Name} {pk.SqlType}");
            sb.AppendLine("AS\nBEGIN");
            sb.AppendLine("    SET NOCOUNT ON;\n");
            sb.AppendLine($"    SELECT {string.Join(", ", selectCols.ConvertAll(c => c.Name))} FROM {tbl} WHERE {pk.Name} = @{pk.Name} AND IsDeleted = 0;");
            sb.AppendLine("END\nGO\n");

            // Update
            sb.AppendLine($"CREATE PROCEDURE sp{objName}_Update");
            sb.AppendLine($"    @{pk.Name} {pk.SqlType},");
            foreach (var c in updateCols)
                sb.AppendLine($"    @{c.Name} {c.SqlType},");
            sb.Remove(sb.Length - 3, 1); // Remove last comma
            sb.AppendLine("AS\nBEGIN");
            sb.AppendLine("    SET NOCOUNT ON;\n");
            sb.AppendLine($"    UPDATE {tbl} SET");
            foreach (var c in updateCols)
                sb.AppendLine($"        {c.Name} = @{c.Name},");
            sb.Remove(sb.Length - 3, 1); // Remove last comma
            if (cols.Any(c => c.Name.ToLower() == "updateddate"))
            {
                sb.AppendLine(",");
                sb.AppendLine("        UpdatedDate = GETDATE()");
            }
            sb.AppendLine($"    WHERE {pk.Name} = @{pk.Name};");
            sb.AppendLine("    IF @@ROWCOUNT > 0 RETURN 1 ELSE RETURN 0");
            sb.AppendLine("END\nGO\n");

            // Delete (Soft Delete)
            //sb.AppendLine($"CREATE PROCEDURE sp{objName}_Delete");
            //sb.AppendLine($"    @{pk.Name} {pk.SqlType},");
            //sb.AppendLine($"    @UpdatedByUserID INT");
            //sb.AppendLine("AS\nBEGIN");
            //sb.AppendLine("    SET NOCOUNT ON;\n");
            //sb.AppendLine($"    UPDATE {tbl} SET IsDeleted = 1, UpdatedByUserID = @UpdatedByUserID, UpdatedDate = GETDATE() WHERE {pk.Name} = @{pk.Name} AND IsDeleted != 1;");
            //sb.AppendLine("    IF @@ROWCOUNT > 0 RETURN 1 ELSE RETURN 0");
            //sb.AppendLine("END\nGO\n");

            //  Generate the Stored Procedure for Delete
            if (cols.Any(c => c.Name.ToLower() == "isdeleted"))
            {
                sb.AppendLine($"CREATE PROCEDURE sp{objName}_Delete");
                sb.Append($"    @{pk.Name} {pk.SqlType}");

                if (cols.Any(c => c.Name.ToLower() == "updatedbyuserid"))
                {
                    sb.AppendLine(",");
                    sb.AppendLine($"    @UpdatedByUserID INT");
                }
                sb.AppendLine();
                sb.AppendLine("AS \nBEGIN");
                sb.AppendLine("    SET NOCOUNT ON;");
                sb.AppendLine("    DECLARE @IsCompleted INT;");
                sb.AppendLine();
                sb.AppendLine("    BEGIN TRY");
                sb.AppendLine("        BEGIN TRANSACTION");
                sb.AppendLine();
                sb.AppendLine("        -- Attempt to delete (Intercepted by Trigger)");
                sb.AppendLine($"        DELETE FROM {tbl}");
                sb.AppendLine($"        WHERE {pk.Name} = @{pk.Name} AND IsDeleted != 1");
                sb.AppendLine();
                sb.AppendLine("        SET @IsCompleted = @@ROWCOUNT;");
                sb.AppendLine("        -- If ID didn't exist or was already deleted");
                sb.AppendLine("        IF @@ROWCOUNT = 0");
                sb.AppendLine("            THROW 51000, 'No record found to delete', 1;");
                sb.AppendLine();
                if (cols.Any(c => c.Name.ToLower() == "updatedbyuserid"))
                {
                    sb.AppendLine("        -- Update audit info");
                    sb.AppendLine($"        UPDATE {tbl}");
                    sb.AppendLine("        SET UpdatedByUserID = @UpdatedByUserID,");
                    sb.AppendLine("            UpdatedDate = GETDATE()");
                    sb.AppendLine($"        WHERE {pk.Name} = @{pk.Name}");
                }
                sb.AppendLine();
                sb.AppendLine("        COMMIT TRANSACTION");
                sb.AppendLine("    END TRY");
                sb.AppendLine("    BEGIN CATCH");
                sb.AppendLine("        IF @@TRANCOUNT > 0");
                sb.AppendLine("            ROLLBACK TRANSACTION;");
                sb.AppendLine("    END CATCH");
                sb.AppendLine();
                sb.AppendLine("    IF @IsCompleted > 0 RETURN 1; ELSE RETURN 0;");
                sb.AppendLine("END");
                sb.AppendLine("GO");
                sb.AppendLine();

                // 2. Generate the Trigger
                sb.AppendLine($"CREATE TRIGGER {objName}SoftDelete");
                sb.AppendLine($"    ON {tbl}");
                sb.AppendLine("    INSTEAD OF DELETE");
                sb.AppendLine("AS \nBEGIN");
                sb.AppendLine("    SET NOCOUNT ON;");
                sb.AppendLine($"    UPDATE {tbl}");
                sb.AppendLine("    SET IsDeleted = 1");
                sb.AppendLine($"    WHERE {pk.Name} IN (SELECT {pk.Name} FROM deleted)");
                sb.AppendLine("      AND IsDeleted != 1");
                sb.AppendLine("END");
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"CREATE PROCEDURE sp{objName}_Delete");
                sb.AppendLine($"    @{pk.Name} {pk.SqlType},");
                if (cols.Any(c => c.Name.ToLower() == "updatedbyuserid"))
                    sb.AppendLine($"    @UpdatedByUserID INT");
                sb.AppendLine("AS \nBEGIN");
                sb.AppendLine("    SET NOCOUNT ON;");
                sb.AppendLine($"    DELETE FROM {tbl} WHERE {pk.Name} = @{pk.Name};");
                sb.AppendLine("    IF @@ROWCOUNT > 0 RETURN 1 ELSE RETURN 0");
                sb.AppendLine("END\nGO\n");

            }


            return sb.ToString();
        }
    }
}
