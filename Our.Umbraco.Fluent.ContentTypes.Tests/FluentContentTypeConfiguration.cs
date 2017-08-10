using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class FluentContentTypeConfiguration
    {
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeConfigurator> documentTypes;

        public FluentContentTypeConfiguration(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            documentTypes = new Dictionary<string, DocumentTypeConfigurator>();
        }

        public Dictionary<string, DocumentTypeConfigurator> DocumentTypes => documentTypes;

        public DataTypeConfiguration DataType(string name)
        {
            var dataTypeConfiguration = new DataTypeConfiguration(this, name);
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