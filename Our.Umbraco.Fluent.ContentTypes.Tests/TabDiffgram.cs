using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class TabDiffgram
    {
        private readonly TabConfigurator config;
        private readonly PropertyGroupCollection propertyGroups;
        public string Name => config.Name;
        public Dictionary<string, PropertyTypeDiffgram> Properties { get; }

        public bool IsNew { get; private set; }

        public TabDiffgram(TabConfigurator config, PropertyGroupCollection propertyGroups)
        {
            this.config = config;
            this.propertyGroups = propertyGroups;
            Properties = new Dictionary<string, PropertyTypeDiffgram>();
        }

        public void Compare()
        {
            var group = propertyGroups.FirstOrDefault(g => g.Name == config.Name);

            IsNew = group == null;

            foreach (var property in config.Properties.Values)
            {
                var propDiff = new PropertyTypeDiffgram(property, group?.PropertyTypes ?? new PropertyTypeCollection(new PropertyType[0]));
                Properties.Add(property.Alias, propDiff);
                propDiff.Compare();
            }
        }
    }
}