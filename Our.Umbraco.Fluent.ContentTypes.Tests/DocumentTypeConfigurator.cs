using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class DocumentTypeConfigurator
    {
        private readonly FluentContentTypeConfiguration parent;
        public DocumentTypeConfiguration Configuration { get; }
        public Dictionary<string, TabConfigurator> Tabs { get; private set; }

        public string Alias => Configuration.Alias;

        public DocumentTypeConfigurator(FluentContentTypeConfiguration parent, string @alias)
        {
            this.parent = parent;
            Configuration = new DocumentTypeConfiguration(alias);
            Tabs = new Dictionary<string, TabConfigurator>();
        }

        public TabConfigurator Tab(string tabName)
        {
            if (Tabs.ContainsKey(tabName))
                return Tabs[tabName];
            return new TabConfigurator(this, tabName);
        }

        public DocumentTypeConfigurator Compositions(params string[] compositions)
        {
            Configuration.Compositions = Configuration.Compositions.Union(compositions);
            return this;
        }

        public DocumentTypeConfigurator Parent(string parent)
        {
            Configuration.Parent = parent;
            return this;
        }

        public DocumentTypeConfigurator ContentType(string alias)
        {
            return parent.ContentType(alias);
        }
    }

    public class DocumentTypeConfiguration
    {
        public string Alias { get; private set; }
        public Dictionary<string, TabConfiguration> Tabs { get; private set; }
        public IEnumerable<string> Compositions { get; set; }
        public string Parent { get; set; }

        public DocumentTypeConfiguration(string alias)
        {
            Alias = alias;
            Tabs = new Dictionary<string, TabConfiguration>();
            Compositions = new string[0];
        }
    }
}