namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class PropertyConfigurator
    {
        private readonly TabConfigurator parent;

        public PropertyConfigurator(TabConfigurator parent, string @alias)
        {
            this.parent = parent;
            Configuration = new PropertyConfiguration(alias);
            parent.Configuration.Properties.Add(alias, Configuration);
        }

        public PropertyConfiguration Configuration { get; private set; }

        public PropertyConfigurator DisplayName(string displayName)
        {
            return this;
        }

        public PropertyConfigurator Description(string description)
        {
            return this;
        }

        public PropertyConfigurator DataType(string dataTypeAlias)
        {
            return this;
        }

        public PropertyConfigurator Property(string alias)
        {
            return parent.Property(alias);
        }
    }

    public class PropertyConfiguration
    {
        public string Alias { get; }

        public PropertyConfiguration(string @alias)
        {
            Alias = alias;
        }
    }
}