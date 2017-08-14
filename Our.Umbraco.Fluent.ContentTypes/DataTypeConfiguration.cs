using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeConfiguration
    {
        public DataTypeConfiguration(FluentContentTypeConfiguration parent, string name)
        {
        }

        public DataTypeConfiguration PropertyEditor(string propertyEditorAlias)
        {
            return this;
        }

        public DataTypeConfiguration DataType(DataTypeDatabaseType type)
        {
            return this;
        }
    }
}