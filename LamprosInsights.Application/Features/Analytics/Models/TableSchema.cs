using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Models
{
   
    public class TableSchema
    {
        public string Name { get; set; } = string.Empty;

        public List<ColumnSchema> Columns { get; set; } = [];
    }
    
}
