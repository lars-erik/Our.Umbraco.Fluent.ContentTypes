using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class PropertyTypeDiffgram
    {
        private readonly PropertyConfiguration config;
        private readonly PropertyTypeCollection propertyCollection;
        private readonly ServiceContext serviceContext;

        public bool IsNew { get; private set; }

        public bool IsUnsafe { get; private set; }

        public PropertyTypeDiffgram(PropertyConfigurator configurator, PropertyTypeCollection propertyCollection, ServiceContext serviceContext)
        {
            this.config = configurator.Configuration;
            this.propertyCollection = propertyCollection;
            this.serviceContext = serviceContext;
        }

        public void Compare()
        {
            var existing = propertyCollection.SingleOrDefault(p => p.Alias == config.Alias);
            var dataType = serviceContext.DataTypeService.GetDataTypeDefinitionByName(config.DataType);

            IsNew = existing == null;

            if (dataType == null)
            {
                IsUnsafe = true;
                return;
            }

            if (!IsNew)
            {
                IsUnsafe =
                    !config.DisplayName.InvariantEquals(existing.Name) ||
                    !config.Description.InvariantEquals(existing.Description) ||
                    dataType.Id != existing.DataTypeDefinitionId;
            }
        }
    }
}