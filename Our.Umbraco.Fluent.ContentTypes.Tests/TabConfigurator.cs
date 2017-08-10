using System.Collections.Generic;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class TabConfigurator
    {
        private readonly DocumentTypeConfigurator parent;
        public TabConfiguration Configuration { get; private set; }

        public Dictionary<string, PropertyConfigurator> Properties { get; }

        public TabConfigurator(DocumentTypeConfigurator parent, string tabName)
        {
            this.parent = parent;
            Properties = new Dictionary<string, PropertyConfigurator>();
            Configuration = new TabConfiguration(tabName);
            parent.Configuration.Tabs.Add(tabName, Configuration);
            parent.Tabs.Add(tabName, this);
        }

        public PropertyConfigurator Property(string alias)
        {
            return new PropertyConfigurator(this, alias);
        }
    }

    public class TabConfiguration
    {
        public string Name { get; private set; }

        public Dictionary<string, PropertyConfiguration> Properties { get; private set; }

        public TabConfiguration(string name)
        {
            Name = name;
            Properties = new Dictionary<string, PropertyConfiguration>();
        }
    }
}