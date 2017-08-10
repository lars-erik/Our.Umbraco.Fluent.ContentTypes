using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class TabDiffgram
    {
        private readonly TabConfigurator configurator;
        private readonly PropertyGroupCollection propertyGroups;
        private readonly ServiceContext serviceContext;
        private readonly TabConfiguration configuration;
        public string Name => configuration.Name;
        public Dictionary<string, PropertyTypeDiffgram> Properties { get; }

        public bool IsNew { get; private set; }

        public TabDiffgram(TabConfigurator configurator, PropertyGroupCollection propertyGroups, ServiceContext serviceContext)
        {
            this.configurator = configurator;
            this.configuration = configurator.Configuration;
            this.propertyGroups = propertyGroups;
            this.serviceContext = serviceContext;
            Properties = new Dictionary<string, PropertyTypeDiffgram>();
        }

        public void Compare()
        {
            var group = propertyGroups.FirstOrDefault(g => g.Name == configuration.Name);

            IsNew = group == null;

            foreach (var property in configurator.Properties.Values)
            {
                var propDiff = new PropertyTypeDiffgram(
                    property, 
                    group?.PropertyTypes ?? new PropertyTypeCollection(new PropertyType[0]),
                    serviceContext
                );
                Properties.Add(property.Alias, propDiff);
                propDiff.Compare();
            }
        }
    }
}