using System.Collections.Generic;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class DocumentTypeConfigurator
    {
        public DocumentTypeConfiguration Configuration { get; }
        public Dictionary<string, TabConfigurator> Tabs { get; private set; }
        public string Alias => Configuration.Alias;

        public DocumentTypeConfigurator(FluentContentTypeConfiguration parent, string @alias)
        {
            Configuration = new DocumentTypeConfiguration(alias);
            Tabs = new Dictionary<string, TabConfigurator>();
        }

        public TabConfigurator Tab(string tabName)
        {
            var tab = new TabConfigurator(this, tabName);
            Tabs.Add(tabName, tab);
            return tab;
        }
    }

    public class DocumentTypeConfiguration
    {
        public string Alias { get; private set; }
        public Dictionary<string, TabConfiguration> Tabs { get; private set; }

        public DocumentTypeConfiguration(string alias)
        {
            Alias = alias;
        }
    }
}