using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;

namespace CodeGeneratorTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // Hardcoded SQL input string
            string sql = @"CREATE TABLE ProductUnits (
    ProductUnitID    INT IDENTITY(1,1) PRIMARY KEY NOT NULL, -- Surrogate key for the specific product+unit combination
    ProductID        INT              NOT NULL,  -- FK to Products(ProductID)
    UnitID           INT              NOT NULL,  -- FK to Units(UnitID)
    
	Description      NVARCHAR (MAX)   NULL,      -- Optional description (e.g., 'Box of 12 pieces')
    ConversionFactor DECIMAL(18, 4)  NOT NULL DEFAULT 1, -- How many base units this unit represents
    SalePrice        DECIMAL(18, 2)  NULL,      -- Retail price per this unit; DECIMAL(18,2) preferred over MONEY to avoid rounding issues
    Barcode          NVARCHAR(50)    NULL,      -- Barcode printed on packaging for POS scanning
    IsActive         BIT             DEFAULT 0, -- 1 = Currently sold, 0 = discontinued for sale

    -- Audit
    CreatedDate      DATETIME        DEFAULT GETDATE(),
    CreatedByUserID  INT             NULL,
    UpdatedDate      DATETIME        DEFAULT GETDATE(),
    UpdatedByUserID  INT             NULL,
    IsDeleted        BIT             DEFAULT 0,
);";

            var table = SqlTableParser.Parse(sql);
            string sqlProcedures = SqlCodeBuilder.GenerateProcedures(table);
            string dalCode = DalCodeBuilder.GenerateDal(table);
            string bllCode = BllCodeBuilder.GenerateBll(table);

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Output");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            File.WriteAllText(Path.Combine(@"F:\Programing Projects\Grocery project\SQL Queries", $"{table.Columns.First(c => c.IsPrimaryKey).Name.Substring(0, table.Columns.First(c => c.IsPrimaryKey).Name.Length - 2)}_Queries.sql"), sqlProcedures);
            File.WriteAllText(Path.Combine(@"F:\Programing Projects\Grocery project\RMS\RMS_DataAccess", $"cls{table.ObjectName}Data.cs"), dalCode);
            File.WriteAllText(Path.Combine(@"F:\Programing Projects\Grocery project\RMS\RMS_Business", $"cls{table.ObjectName}.cs"), bllCode);
            Console.WriteLine("Files generated in Output folder.");
        }
    }

}
