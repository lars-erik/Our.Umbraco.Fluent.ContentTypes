namespace Our.Umbraco.Fluent.ContentTypes
{
    public class PropertyConfigurator
    {
        private readonly TabConfigurator parent;

        public PropertyConfiguration Configuration { get; private set; }

        public string Alias => Configuration.Alias;

        public PropertyConfigurator(TabConfigurator parent, string @alias)
        {
            this.parent = parent;
            Configuration = new PropertyConfiguration(alias);
            parent.Properties.Add(alias, this);
            parent.Configuration.Properties.Add(alias, Configuration);
        }

        public PropertyConfigurator DisplayName(string displayName)
        {
            Configuration.Name = displayName;
            return this;
        }

        public PropertyConfigurator Description(string description)
        {
            Configuration.Description = description;
            return this;
        }

        public PropertyConfigurator DataType(string dataTypeAlias)
        {
            Configuration.DataType = dataTypeAlias;
            return this;
        }

        public PropertyConfigurator Property(string alias)
        {
            return parent.Property(alias);
        }

        public TabConfigurator Tab(string name)
        {
            return parent.Tab(name);
        }

        public PropertyConfigurator Mandatory(bool value = true)
        {
            Configuration.Mandatory = value;
            return this;
        }

        public PropertyConfigurator Regex(string pattern)
        {
            Configuration.ValidationRegExp = pattern;
            return this;
        }
    }

    public class PropertyConfiguration
    {
        public string Alias { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool Mandatory { get; set; }
        public string ValidationRegExp { get; set; }

        public PropertyConfiguration(string @alias)
        {
            Alias = alias;
        }
    }
}