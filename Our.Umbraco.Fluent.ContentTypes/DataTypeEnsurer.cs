using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeEnsurer
    {
        private readonly DataTypeDiffgram dataTypeDiffgram;
        private readonly ServiceContext serviceContext;

        public DataTypeEnsurer(DataTypeDiffgram dataTypeDiffgram, ServiceContext serviceContext)
        {
            this.dataTypeDiffgram = dataTypeDiffgram;
            this.serviceContext = serviceContext;
        }

        public void Ensure()
        {
            dataTypeDiffgram.AssertSafe();

            IDataTypeDefinition datatype;
            var datatypeConfig = dataTypeDiffgram.Configuration;

            var newPrevalueComparisons = dataTypeDiffgram.Comparisons
                .Where(comparison => comparison.Key == "Prevalues" && comparison.Result == ComparisonResult.New)
                .Select(comparison => comparison.Discriminator);
            var newPrevaluePairs = datatypeConfig.Prevalues.Where(kvp => newPrevalueComparisons.Contains(kvp.Key));
            var newPrevalues = newPrevaluePairs.ToDictionary(kvp => kvp.Key, kvp => new PreValue(kvp.Value));

            if (dataTypeDiffgram.IsNew)
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
                datatype = serviceContext.DataTypeService.GetDataTypeDefinitionByName(dataTypeDiffgram.Key);
                var extPrevalues = serviceContext.DataTypeService.GetPreValuesCollectionByDataTypeId(datatype.Id);
                newPrevalues = extPrevalues.PreValuesAsDictionary.Union(newPrevalues).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                serviceContext.DataTypeService.SavePreValues(datatype, newPrevalues);
            }
        }
    }
}