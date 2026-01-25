# CodeGeneratorTool

**CodeGeneratorTool** is a lightweight C# console application that parses a SQL `CREATE TABLE` statement and automatically generates:

- **SQL Stored Procedures** (`AddNew`, `GetAll`, `GetByID`, `Update`, `Delete`)
- **Data Access Layer (DAL)** class
- **Business Logic Layer (BLL)** class

It is designed for **SQL Server** and follows a clean, consistent architecture for CRUD operations.

---

## 📌 Features

✅ Parses table schema from a SQL `CREATE TABLE` statement

✅ Generates:
- `sp{Object}_AddNew`
- `sp{Object}_GetAll`
- `sp{Object}_GetByID`
- `sp{Object}_Update`
- `sp{Object}_Delete`

✅ Generates DAL class with:
- `GetByID`
- `AddNew`
- `Update`
- `Delete`
- `GetAll`

✅ Generates BLL class with:
- Model properties
- `Save()` method (Add/Update based on mode)
- `Find()` method
- `Delete()` method
- `GetAll()` method

✅ Handles soft delete if the table contains `IsDeleted` column

---

## 🧩 Prerequisites

Make sure you have:

- **.NET 6.0+ SDK**
- **SQL Server** (local or remote)
- **Microsoft.Data.SqlClient** package installed

You can install the package using:

```bash
dotnet add package Microsoft.Data.SqlClient
```

---

## 🧠 How It Works

The tool reads a hardcoded SQL `CREATE TABLE` statement inside `Program.cs`, parses it using `SqlTableParser`, then generates:

1. SQL Stored Procedures (`SqlCodeBuilder`)
2. DAL class (`DalCodeBuilder`)
3. BLL class (`BllCodeBuilder`)

Finally, it writes the generated files to your project folders.

---

## 🚀 Usage Guide

### 1. Update the SQL Input

Open `Program.cs` and update the `sql` variable with your table definition.

Example:

```csharp
string sql = @"CREATE TABLE ProductUnits (
    ProductUnitID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    ProductID INT NOT NULL,
    UnitID INT NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ConversionFactor DECIMAL(18, 4) NOT NULL DEFAULT 1,
    SalePrice DECIMAL(18, 2) NULL,
    Barcode NVARCHAR(50) NULL,
    IsActive BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedByUserID INT NULL,
    UpdatedDate DATETIME DEFAULT GETDATE(),
    UpdatedByUserID INT NULL,
    IsDeleted BIT DEFAULT 0,
);";
```

> ⚠️ Make sure the SQL statement is valid and ends with `);`.

---

### 2. Run the Tool

Run the project using Visual Studio or CLI:

```bash
dotnet run
```

The tool will generate:

- `*_Queries.sql` in:
  `F:\Programing Projects\Grocery project\SQL Queries`

- DAL class in:
  `F:\Programing Projects\Grocery project\RMS\RMS_DataAccess`

- BLL class in:
  `F:\Programing Projects\Grocery project\RMS\RMS_Business`

---

## 📁 Output Files

### 1. SQL Stored Procedures
Generated SQL file name format:

```
{ObjectName}_Queries.sql
```

Example:

```
ProductUnit_Queries.sql
```

### 2. DAL Class
Generated DAL class name format:

```
cls{ObjectName}Data.cs
```

Example:

```
clsProductUnitData.cs
```

### 3. BLL Class
Generated BLL class name format:

```
cls{ObjectName}.cs
```

Example:

```
clsProductUnit.cs
```

---

## 🔧 Project Structure (Recommended)

```
CodeGeneratorTool
│
├─ Program.cs
├─ SqlTableParser.cs
├─ SqlCodeBuilder.cs
├─ DalCodeBuilder.cs
├─ BllCodeBuilder.cs
└─ Output (optional)
```

> Tip: You can split the builders into separate files for better maintainability.

---

## ⚙️ How to Use Generated Code

### 1. Add Generated SQL to Database

Run the generated SQL file (`*_Queries.sql`) in SQL Server Management Studio (SSMS) to create the stored procedures.

### 2. Add DAL & BLL to Your Project

Copy the generated classes to your `RMS_DataAccess` and `RMS_Business` projects.

### 3. Use in Code

```csharp
var productUnit = new clsProductUnit();
productUnit.ProductID = 1;
productUnit.UnitID = 2;
productUnit.Description = "Box of 12";
productUnit.ConversionFactor = 12;
productUnit.Mode = clsProductUnit.enMode.AddNew;
productUnit.Save();
```

---

## ✅ Notes & Limitations

### ✅ Supported SQL Types
- `INT`, `BIGINT`, `SMALLINT`, `TINYINT`
- `BIT`
- `DECIMAL`, `NUMERIC`, `MONEY`, `SMALLMONEY`
- `FLOAT`, `REAL`
- `DATE`, `DATETIME`, `SMALLDATETIME`
- `UNIQUEIDENTIFIER`
- `VARCHAR`, `NVARCHAR`, `CHAR`, `NCHAR`, `TEXT`, `NTEXT`
- `VARBINARY`, `BINARY`, `IMAGE`

### ⚠️ Limitations
- Computed columns are detected but not supported in CRUD operations.
- SQL parsing assumes a simple `CREATE TABLE` format (no schema prefixes, no inline constraints other than PK).
- `SqlDbType` mapping is basic and may need expansion for complex types.

---

## 🛠️ Customization Tips

### 1. Change Output Paths
Edit `Program.cs`:

```csharp
string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Output");
// change to your desired path
```

### 2. Add Support for More SQL Types
Update `SqlTypeMapper.ToCSharpType()` and `SqlColumn.SqlTypeEnum`.

### 3. Add Schema Support
Update the parser to capture schema name (`dbo.TableName`).

---

## 🧪 Troubleshooting

### ❌ “Invalid column name” errors
Ensure the SQL column names match the database table.

### ❌ Stored procedures not found
Make sure you executed the generated SQL file in the correct database.

### ❌ Null values crash `GetByID`
The DAL handles nulls only for nullable types. Ensure your table columns are marked `NULL` properly.

---

## 📌 Contact / Support

If you need enhancements (e.g., relationships, foreign keys, pagination, filtering, etc.), feel free to ask.

---

Happy coding! 🎯

