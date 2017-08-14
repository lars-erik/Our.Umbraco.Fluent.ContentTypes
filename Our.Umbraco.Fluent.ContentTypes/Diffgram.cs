using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class Diffgram
    {
        private readonly FluentContentTypeConfiguration configuration;
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeDiffgram> docTypes;
        private readonly Dictionary<string, DataTypeDiffgram> dataTypes;
        private IContentTypeService contentTypeService;

        public Diffgram(FluentContentTypeConfiguration configuration, ServiceContext serviceContext)
        {
            this.configuration = configuration;
            this.serviceContext = serviceContext;
            contentTypeService = serviceContext.ContentTypeService;

            docTypes = new Dictionary<string, DocumentTypeDiffgram>();
            dataTypes = new Dictionary<string, DataTypeDiffgram>();
        }

        public bool Safe { get; private set; }

        public Dictionary<string, DocumentTypeDiffgram> DocumentTypes => docTypes;
        public Dictionary<string, DataTypeDiffgram> DataTypes => dataTypes;

        public void Compare()
        {
            foreach (var docType in configuration.DocumentTypes)
            {
                DocumentTypeConfigurator docTypeConfig = docType.Value;
                var docTypeDiff = AddDocumentType(docTypeConfig.Configuration);
                docTypeDiff.Compare();
            }

            foreach (var dataTypes in configuration.DataTypes)
            {
                DataTypeConfigurator dataTypeConfig = dataTypes.Value;
                var datatypeDiff = AddDataType(dataTypeConfig.Configuration);
                datatypeDiff.Compare();
            }

            Safe = docTypes.All(t => !t.Value.IsUnsafe) 
                && dataTypes.All(t => !t.Value.IsUnsafe);
        }

        private DataTypeDiffgram AddDataType(DataTypeConfiguration dataTypeConfiguration)
        {
            var datatypeDiffgram = new DataTypeDiffgram(this, dataTypeConfiguration, serviceContext);
            dataTypes.Add(dataTypeConfiguration.Name, datatypeDiffgram);
            return datatypeDiffgram;
        }

        private DocumentTypeDiffgram AddDocumentType(DocumentTypeConfiguration documentTypeConfiguration)
        {
            var docTypeDiffgram = new DocumentTypeDiffgram(this, documentTypeConfiguration, serviceContext);
            docTypes.Add(documentTypeConfiguration.Alias, docTypeDiffgram);
            return docTypeDiffgram;
        }
    }
}