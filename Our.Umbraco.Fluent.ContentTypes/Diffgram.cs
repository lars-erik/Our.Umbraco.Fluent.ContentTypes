using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class Diffgram
    {
        private readonly FluentContentTypeConfiguration configuration;
        private readonly ServiceContext serviceContext;
        private IContentTypeService contentTypeService;

        public bool Safe { get; private set; }

        public Dictionary<string, DocumentTypeDiffgram> DocumentTypes { get; }

        public Dictionary<string, DataTypeDiffgram> DataTypes { get; }

        public Dictionary<string, TemplateDiffgram> Templates { get; }

        public Diffgram(FluentContentTypeConfiguration configuration, ServiceContext serviceContext)
        {
            this.configuration = configuration;
            this.serviceContext = serviceContext;
            contentTypeService = serviceContext.ContentTypeService;

            DocumentTypes = new Dictionary<string, DocumentTypeDiffgram>();
            DataTypes = new Dictionary<string, DataTypeDiffgram>();
            Templates = new Dictionary<string, TemplateDiffgram>();
        }

        public void Compare()
        {
            foreach (var docType in configuration.DocumentTypes.Values)
            {
                var docTypeDiff = AddDocumentType(docType.Configuration);
                docTypeDiff.Compare();
            }

            foreach (var dataType in configuration.DataTypes.Values)
            {
                var datatypeDiff = AddDataType(dataType.Configuration);
                datatypeDiff.Compare();
            }

            foreach (var template in configuration.Templates.Values)
            {
                var templateDiff = AddTemplate(template.Configuration);
                templateDiff.Compare();
            }

            Safe = DocumentTypes.All(t => !t.Value.IsUnsafe) 
                && DataTypes.All(t => !t.Value.IsUnsafe)
                && Templates.All(t => !t.Value.IsUnsafe);
        }

        private TemplateDiffgram AddTemplate(TemplateConfiguration templateConfiguration)
        {
            var templateDiffgram = new TemplateDiffgram(this, templateConfiguration, serviceContext);
            Templates.Add(templateConfiguration.Alias, templateDiffgram);
            return templateDiffgram;
        }

        private DataTypeDiffgram AddDataType(DataTypeConfiguration dataTypeConfiguration)
        {
            var datatypeDiffgram = new DataTypeDiffgram(this, dataTypeConfiguration, serviceContext);
            DataTypes.Add(dataTypeConfiguration.Name, datatypeDiffgram);
            return datatypeDiffgram;
        }

        private DocumentTypeDiffgram AddDocumentType(DocumentTypeConfiguration documentTypeConfiguration)
        {
            var docTypeDiffgram = new DocumentTypeDiffgram(this, documentTypeConfiguration, serviceContext);
            DocumentTypes.Add(documentTypeConfiguration.Alias, docTypeDiffgram);
            return docTypeDiffgram;
        }
    }
}