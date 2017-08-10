using System.Linq;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class PropertyTypeDiffgram
    {
        private readonly PropertyConfigurator config;
        private readonly PropertyTypeCollection propertyCollection;

        public bool IsNew { get; private set; }

        public bool IsUnsafe { get; private set; }

        public PropertyTypeDiffgram(PropertyConfigurator config, PropertyTypeCollection propertyCollection)
        {
            this.config = config;
            this.propertyCollection = propertyCollection;
        }

        public void Compare()
        {
            var existing = propertyCollection.SingleOrDefault(p => p.Alias == config.Alias);

            IsNew = existing == null;

            if (!IsNew)
            {
                //IsUnsafe =
                //    config.DisplayName.
            }
        }
    }
}