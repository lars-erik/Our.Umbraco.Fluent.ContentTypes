using System.Collections.Generic;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class TabConfigurator
    {
        public TabConfiguration Configuration { get; private set; }

        public Dictionary<string, PropertyConfigurator> Properties { get; }

        public TabConfigurator(DocumentTypeConfigurator parent, string tabName)
        {
            Properties = new Dictionary<string, PropertyConfigurator>();
            Configuration = new TabConfiguration(tabName);
            parent.Configuration.Tabs.Add(tabName, Configuration);
        }

        public PropertyConfigurator Property(string alias)
        {
            var property = new PropertyConfigurator(this, alias);
            Properties.Add(property.Alias, property);
            return property;
        }
    }

    public class TabConfiguration
    {
        public string Name { get; private set; }

        public Dictionary<string, object> Properties { get; private set; }

        public TabConfiguration(string name)
        {
            Name = name;
            Properties = new Dictionary<string, object>();
        }
    }
}