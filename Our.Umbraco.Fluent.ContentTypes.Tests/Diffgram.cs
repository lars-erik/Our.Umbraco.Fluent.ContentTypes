using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class Diffgram
    {
        private readonly FluentContentTypeConfiguration configuration;
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeDiffgram> docTypes;
        private IContentTypeService contentTypeService;

        public Diffgram(FluentContentTypeConfiguration configuration, ServiceContext serviceContext)
        {
            this.configuration = configuration;
            this.serviceContext = serviceContext;
            contentTypeService = serviceContext.ContentTypeService;

            docTypes = new Dictionary<string, DocumentTypeDiffgram>();
        }

        public bool Safe { get; set; }

        public Dictionary<string, DocumentTypeDiffgram> DocumentTypes => docTypes;

        public void Compare()
        {
            foreach (var docType in configuration.DocumentTypes)
            {
                DocumentTypeConfigurator docTypeConfig = docType.Value;
                var docTypeDiff = AddDocumentType(docTypeConfig.Configuration);
                docTypeDiff.Compare();
            }
        }

        private DocumentTypeDiffgram AddDocumentType(DocumentTypeConfiguration documentTypeConfiguration)
        {
            var docTypeDiffgram = new DocumentTypeDiffgram(this, documentTypeConfiguration, serviceContext);
            docTypes.Add(documentTypeConfiguration.Alias, docTypeDiffgram);
            return docTypeDiffgram;
        }
    }
}