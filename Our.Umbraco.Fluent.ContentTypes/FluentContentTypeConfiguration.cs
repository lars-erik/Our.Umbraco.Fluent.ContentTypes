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

        public DocumentTypeConfigurator ContentType(string alias)
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
            foreach (var datatypeDiff in diffgram.DataTypes.Values)
            {
                IDataTypeDefinition datatype;
                var datatypeConfig = datatypeDiff.Configuration;

                var newPrevalueComparisons = datatypeDiff
                    .Comparisons
                    .Where(comparison => comparison.Key == "Prevalues" && comparison.Result == ComparisonResult.New)
                    .Select(comparison => comparison.Discriminator);
                var newPrevaluePairs = datatypeConfig.Prevalues.Where(kvp => newPrevalueComparisons.Contains(kvp.Key));
                var newPrevalues = newPrevaluePairs.ToDictionary(kvp => kvp.Key, kvp => new PreValue(kvp.Value));

                if (datatypeDiff.IsNew)
                {
                    datatype = new DataTypeDefinition(datatypeConfig.PropertyEditorAlias)
                    {
                        Name = datatypeConfig.Name,
                        DatabaseType = datatypeConfig.DatabaseType
                    };
                    serviceContext.DataTypeService.SaveDataTypeAndPreValues(datatype, newPrevalues);
                }
                else
                {
                    datatype = serviceContext.DataTypeService.GetDataTypeDefinitionByName(datatypeDiff.Key);
                    var extPrevalues = serviceContext.DataTypeService.GetPreValuesCollectionByDataTypeId(datatype.Id);
                    newPrevalues = extPrevalues.PreValuesAsDictionary.Union(newPrevalues).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    serviceContext.DataTypeService.SavePreValues(datatype, newPrevalues);
                }
            }
        }
    }
}