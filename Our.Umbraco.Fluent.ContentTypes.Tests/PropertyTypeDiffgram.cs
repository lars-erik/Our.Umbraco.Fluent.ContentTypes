using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class PropertyTypeDiffgram : EntityDiffgram<PropertyConfiguration, PropertyType>
    {
        private readonly PropertyTypeCollection propertyCollection;
        private IDataTypeDefinition dataTypeDefinition;

        public override string Key => Configuration.Alias;

        public PropertyTypeDiffgram(PropertyConfiguration configuration, PropertyTypeCollection propertyCollection, ServiceContext serviceContext)
            : base(configuration, serviceContext)
        {
            this.propertyCollection = propertyCollection;
        }

        protected override bool ValidateConfiguration()
        {
            dataTypeDefinition = ServiceContext.DataTypeService.GetDataTypeDefinitionByName(Configuration.DataType);

            return dataTypeDefinition != null;
        }

        protected override void CompareToExisting()
        {
            base.CompareToExisting();

            if (!IsModified)
            {
                IsModified = IsUnsafe = dataTypeDefinition.Id != Existing.DataTypeDefinitionId;
            }
        }

        protected override PropertyType FindExisting()
        {
            return propertyCollection?.SingleOrDefault(p => p.Alias == Configuration.Alias);
        }
    }
}