using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeConfiguration
    {
        public string Name { get; }
        public string PropertyEditorAlias { get; set; }
        public DataTypeDatabaseType DatabaseType { get; set; }
        public Dictionary<string, string> Prevalues { get; }

        public DataTypeConfiguration(string name)
        {
            Name = name;
            Prevalues = new Dictionary<string, string>();
        }
    }
}
