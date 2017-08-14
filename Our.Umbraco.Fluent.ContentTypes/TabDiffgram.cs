using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class TabDiffgram : EntityDiffgram<TabConfiguration, PropertyGroup>
    {
        private readonly PropertyGroupCollection propertyGroups;
        public override string Key => Configuration.Name;
        public Dictionary<string, PropertyTypeDiffgram> Properties { get; }

        public TabDiffgram(TabConfiguration configuration, PropertyGroupCollection propertyGroups, ServiceContext serviceContext)
            : base(configuration, serviceContext)
        {
            this.propertyGroups = propertyGroups;
            Properties = new Dictionary<string, PropertyTypeDiffgram>();
        }

        protected override PropertyGroup FindExisting()
        {
            return propertyGroups?.FirstOrDefault(g => g.Name == Configuration.Name);
        }

        protected override void CompareChildren()
        {
            foreach (var property in Configuration.Properties.Values)
            {
                var propDiff = new PropertyTypeDiffgram(
                    property, 
                    Existing?.PropertyTypes,
                    ServiceContext
                );
                Properties.Add(property.Alias, propDiff);
                propDiff.Compare();
            }
        }
    }
}