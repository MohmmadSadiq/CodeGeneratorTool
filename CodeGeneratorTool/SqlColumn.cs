using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public class SqlColumn
    {
        public string Name { get; set; } = "";
        public string SqlType { get; set; } = ""; // المتغير النصي القادم من قاعدة البيانات
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public string? ComputedExpression { get; set; }
        public string? DefaultValue { get; set; }

        // هذه هي الخاصية الجديدة
        public SqlDbType SqlTypeEnum
        {
            get
            {
                // تنظيف النص وتوحيد حالة الأحرف لتجنب الأخطاء
                return SqlType.ToLowerInvariant() switch
                {
                    "bigint" => SqlDbType.BigInt,
                    "binary" => SqlDbType.Binary,
                    "bit" => SqlDbType.Bit,
                    "char" => SqlDbType.Char,
                    "date" => SqlDbType.Date,
                    "datetime" => SqlDbType.DateTime,
                    "datetime2" => SqlDbType.DateTime2,
                    "datetimeoffset" => SqlDbType.DateTimeOffset,
                    "decimal" => SqlDbType.Decimal,
                    "float" => SqlDbType.Float,
                    "image" => SqlDbType.Image,
                    "int" => SqlDbType.Int,
                    "money" => SqlDbType.Money,
                    "nchar" => SqlDbType.NChar,
                    "ntext" => SqlDbType.NText,
                    "numeric" => SqlDbType.Decimal, // numeric هو نفسه decimal
                    "nvarchar" => SqlDbType.NVarChar,
                    "real" => SqlDbType.Real,
                    "smalldatetime" => SqlDbType.SmallDateTime,
                    "smallint" => SqlDbType.SmallInt,
                    "smallmoney" => SqlDbType.SmallMoney,
                    "text" => SqlDbType.Text,
                    "time" => SqlDbType.Time,
                    "timestamp" => SqlDbType.Timestamp,
                    "tinyint" => SqlDbType.TinyInt,
                    "uniqueidentifier" => SqlDbType.UniqueIdentifier,
                    "varbinary" => SqlDbType.VarBinary,
                    "varchar" => SqlDbType.VarChar,
                    "xml" => SqlDbType.Xml,

                    // القيمة الافتراضية في حال لم يتم التعرف على النوع
                    _ => SqlDbType.VarChar
                };
            }
        }
    }
}
