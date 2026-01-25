using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorTool
{
    public class SqlTable
    {
        public string Name { get; set; } = "";
        public string ObjectName
        {
            get
            {
                return Columns.First(c => c.IsPrimaryKey).Name.Substring(0, Columns.First(c => c.IsPrimaryKey).Name.Length - 2);
            }
        }
        public List<SqlColumn> Columns { get; set; } = new List<SqlColumn>();
    }
}
