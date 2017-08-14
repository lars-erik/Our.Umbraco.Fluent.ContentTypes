using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeConfigurator
    {
        private readonly DataTypeConfiguration configuration;

        public DataTypeConfiguration Configuration => configuration;

        public DataTypeConfigurator(FluentContentTypeConfiguration parent, string name)
        {
            configuration = new DataTypeConfiguration(name);
        }

        public DataTypeConfigurator PropertyEditor(string propertyEditorAlias)
        {
            configuration.PropertyEditorAlias = propertyEditorAlias;
            return this;
        }

        public DataTypeConfigurator DataType(DataTypeDatabaseType type)
        {
            configuration.DatabaseType = type;
            return this;
        }

        public DataTypeConfigurator Prevalue(string alias, string value)
        {
            if (configuration.Prevalues.ContainsKey(alias))
                configuration.Prevalues[alias] = value;
            else
                configuration.Prevalues.Add(alias, value);

            return this;
        }
    }
}