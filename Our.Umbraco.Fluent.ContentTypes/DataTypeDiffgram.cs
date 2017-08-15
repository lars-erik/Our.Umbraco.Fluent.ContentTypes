using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes
{
    public class DataTypeDiffgram : EntityDiffgram<DataTypeConfiguration, IDataTypeDefinition>
    {
        private readonly Diffgram diffgram;
        private readonly IDataTypeService dataTypeService;

        public DataTypeDiffgram(Diffgram diffgram, DataTypeConfiguration configuration, ServiceContext serviceContext) : base(configuration, serviceContext)
        {
            this.diffgram = diffgram;
            dataTypeService = serviceContext.DataTypeService;
        }

        public override string Key => Configuration.Name;

        protected override IDataTypeDefinition FindExisting()
        {
            return dataTypeService.GetDataTypeDefinitionByName(Configuration.Name);
        }

        protected override void CompareChildren()
        {
            base.CompareChildren();

            if (!IsNew)
            {
                var prevalues = dataTypeService.GetPreValuesCollectionByDataTypeId(Existing.Id).PreValuesAsDictionary;
                Comparisons.AddRange(Configuration.Prevalues.Select(kvp => CreateExistingComparison(kvp, prevalues)));
            }
            else
            {
                Comparisons.AddRange(Configuration.Prevalues.Select(kvp => new Comparison("Prevalues", kvp.Key, ComparisonResult.New)));
            }

            IsModified = Comparisons.Any(c => c.Key == "Prevalues" && (c.Result == ComparisonResult.Modified || c.Result == ComparisonResult.New));
            IsUnsafe = Comparisons.Any(c => c.Key == "Prevalues" && c.Result == ComparisonResult.Modified);
        }

        private static Comparison CreateExistingComparison(KeyValuePair<string, string> kvp, IDictionary<string, PreValue> prevalues)
        {
            var extPreValue = prevalues.ContainsKey(kvp.Key) ? prevalues[kvp.Key].Value : null;
            var value = kvp.Value;

            // TODO: Test nullish values
            extPreValue = String.IsNullOrWhiteSpace(extPreValue) ? null : extPreValue;
            value = String.IsNullOrWhiteSpace(value) ? null : value;

            if (value != null && extPreValue == null)
                return new Comparison("Prevalues", kvp.Key, ComparisonResult.New);

            if (extPreValue != value)
                return new Comparison("Prevalues", kvp.Key, ComparisonResult.Modified);

            return new Comparison("Prevalues", kvp.Key, ComparisonResult.Unchanged);
        }
    }
}
