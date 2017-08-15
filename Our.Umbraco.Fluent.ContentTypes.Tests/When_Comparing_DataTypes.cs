using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    [UseReporter(typeof(VisualStudioReporter))]
    public class When_Comparing_DataTypes : ComparisonTestBase
    {
        private const string DataTypeName = "Nested Content Definition";
        private string nestedContentContentTypesPreValue = @"[
                  {
                    ""ncAlias"": ""thisCantBeValidatedForNow"",
                    ""ncTabAlias"": ""The tab to use"",
                    ""nameTemplate"": ""{{someFieldNotValidatedEither}}""
                  }
                ]";
        private Dictionary<string, PreValue> prevalueDictionary;

        [SetUp]
        public void Setup()
        {
            var dataTypeId = 1;

            var dataType = StubDataType(dataTypeId, DataTypeName);
            dataType.PropertyEditorAlias = "Our.Umbraco.NestedContent";
            prevalueDictionary = new Dictionary<string, PreValue>()
            {
                { "contentTypes", new PreValue(1, nestedContentContentTypesPreValue) },
                { "minItems", new PreValue(2, "0") },
                { "maxItems", new PreValue(3, "0") },
                { "confirmDeletes", new PreValue(4, "1") },
                { "showIcons", new PreValue(5, "1") },
                { "hideLabel", new PreValue(6, "") },
            };
            Mock.Get(Support.ServiceContext.DataTypeService).Setup(s => s.GetPreValuesCollectionByDataTypeId(dataTypeId))
                .Returns(() => new PreValueCollection(prevalueDictionary));
            
            Config.DataType(DataTypeName)
                .PropertyEditor("Our.Umbraco.NestedContent")
                .DataType(DataTypeDatabaseType.Ntext)
                .Prevalue("contentTypes", nestedContentContentTypesPreValue)
                .Prevalue("minItems", "0")
                .Prevalue("maxItems", "0")
                .Prevalue("confirmDeletes", "1")
                .Prevalue("showIcons", "1")
                .Prevalue("hideLabel", "");
        }

        [Test]
        public void For_Existing_Equal_Then_Is_Safe()
        {
            VerifyDiffgram();
        }

        [Test]
        public void For_Existing_With_Modified_Prevalue_Then_Is_Unsafe()
        {
            prevalueDictionary["maxItems"] = new PreValue("1");

            VerifyDiffgram();
        }

        [Test]
        public void For_New_Then_Is_Safe()
        {
            Mock.Get(Support.ServiceContext.DataTypeService).Reset();
            Mock.Get(Support.ServiceContext.DataTypeService).Setup(s => s.GetDataTypeDefinitionByName(DataTypeName)).Returns<IDataTypeDefinition>(null);
            
            VerifyDiffgram();
        }

        private void VerifyDiffgram()
        {
            Config.Compare().DataTypes[DataTypeName].Verify();
        }
    }
}
