using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public interface IConfigurator<out TConfiguration>
    {
        TConfiguration Configuration { get; }
    }

    public interface IConfiguratorWithChildren<out TConfiguration, TChildren> : IConfigurator<TConfiguration>
    {
        Dictionary<string, IConfigurator<TChildren>> Children { get; }
    }

    public class DocumentTypeConfigurator : IConfiguratorWithChildren<DocumentTypeConfiguration, TabConfiguration>
    {
        public DocumentTypeConfiguration Configuration { get; }
        public Dictionary<string, TabConfigurator> Tabs { get; private set; }

        public Dictionary<string, IConfigurator<TabConfiguration>> Children
        {
            get { return Tabs.Values.Cast<IConfigurator<TabConfiguration>>().ToDictionary(t => t.Configuration.Name); }
        }

        public string Alias => Configuration.Alias;

        public DocumentTypeConfigurator(FluentContentTypeConfiguration parent, string @alias)
        {
            Configuration = new DocumentTypeConfiguration(alias);
            Tabs = new Dictionary<string, TabConfigurator>();
        }

        public TabConfigurator Tab(string tabName)
        {
            if (Tabs.ContainsKey(tabName))
                return Tabs[tabName];
            return new TabConfigurator(this, tabName);
        }
    }

    public class DocumentTypeConfiguration
    {
        public string Alias { get; private set; }
        public Dictionary<string, TabConfiguration> Tabs { get; private set; }

        public DocumentTypeConfiguration(string alias)
        {
            Alias = alias;
            Tabs = new Dictionary<string, TabConfiguration>();
        }
    }
}