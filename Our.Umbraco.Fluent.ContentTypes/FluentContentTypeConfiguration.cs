using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class FluentContentTypeConfiguration
    {
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeConfigurator> documentTypes;
        private readonly Dictionary<string, DataTypeConfigurator> dataTypes;

        public FluentContentTypeConfiguration(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            documentTypes = new Dictionary<string, DocumentTypeConfigurator>();
            dataTypes = new Dictionary<string, DataTypeConfigurator>();
        }

        public Dictionary<string, DocumentTypeConfigurator> DocumentTypes => documentTypes;
        public Dictionary<string, DataTypeConfigurator> DataTypes => dataTypes;

        public DataTypeConfigurator DataType(string name)
        {
            var dataTypeConfiguration = new DataTypeConfigurator(this, name);
            dataTypes.Add(name, dataTypeConfiguration);
            return dataTypeConfiguration;
        }

        public DocumentTypeConfigurator ContentType(string alias)
        {
            var documentTypeConfiguration = new DocumentTypeConfigurator(this, alias);
            documentTypes.Add(alias, documentTypeConfiguration);
            return documentTypeConfiguration;
        }

        public Diffgram Compare()
        {
            var diffgram = new Diffgram(this, serviceContext);
            diffgram.Compare();
            return diffgram;
        }

        public void Ensure(Diffgram diffgram)
        {

        }
    }
}