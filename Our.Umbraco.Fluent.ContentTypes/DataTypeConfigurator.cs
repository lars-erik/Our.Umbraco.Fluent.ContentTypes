using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeConfigurator
    {
        public DataTypeConfigurator(FluentContentTypeConfiguration parent, string name)
        {
        }

        public DataTypeConfigurator PropertyEditor(string propertyEditorAlias)
        {
            return this;
        }

        public DataTypeConfigurator DataType(DataTypeDatabaseType type)
        {
            return this;
        }
    }
}