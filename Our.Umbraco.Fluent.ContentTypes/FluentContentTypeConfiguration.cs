using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class FluentContentTypeConfiguration
    {
        private readonly ServiceContext serviceContext;

        public Dictionary<string, DocumentTypeConfigurator> DocumentTypes { get; }

        public Dictionary<string, DataTypeConfigurator> DataTypes { get; }

        public Dictionary<string, TemplateConfigurator> Templates { get; }

        public FluentContentTypeConfiguration(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            DocumentTypes = new Dictionary<string, DocumentTypeConfigurator>();
            DataTypes = new Dictionary<string, DataTypeConfigurator>();
            Templates = new Dictionary<string, TemplateConfigurator>();
        }

        public DataTypeConfigurator DataType(string name)
        {
            var dataTypeConfiguration = new DataTypeConfigurator(this, name);
            DataTypes.Add(name, dataTypeConfiguration);
            return dataTypeConfiguration;
        }

        public DocumentTypeConfigurator DocumentType(string alias)
        {
            var documentTypeConfiguration = new DocumentTypeConfigurator(this, alias);
            DocumentTypes.Add(alias, documentTypeConfiguration);
            return documentTypeConfiguration;
        }

        public TemplateConfigurator Template(string alias)
        {
            var configurator = new TemplateConfigurator(this, alias);
            Templates.Add(alias, configurator);
            return configurator;
        }

        public Diffgram Compare()
        {
            var diffgram = new Diffgram(this, serviceContext);
            diffgram.Compare();
            return diffgram;
        }

        public void Ensure(Diffgram diffgram)
        {
            // Sort here to detect circular dependencies. Move to Safe check?
            var orderedDocTypes = DependencyComparer.OrderByDependencies(diffgram.DocumentTypes.Values);

            foreach (var datatypeDiff in diffgram.DataTypes.Values)
            {
                datatypeDiff.Ensure();
            }

            foreach (var templateDiff in diffgram.Templates.Values)
            {
                templateDiff.Ensure();
            }

            foreach (var doctypeDiff in orderedDocTypes)
            {
                doctypeDiff.Ensure();
            }
        }
    }
}