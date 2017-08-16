using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    [UseReporter(typeof(VisualStudioReporter))]
    public class When_Ensuring_DataTypes : ComparisonTestBase
    {
        private Mock<IDataTypeService> datatypeServiceMock;

        [SetUp]
        public void Setup()
        {
            Config.DataType("A datatype")
                .PropertyEditor("nice.editor")
                .Prevalue("x", "y")
                .Prevalue("a", "b");

            datatypeServiceMock = Mock.Get(Support.ServiceContext.DataTypeService);
        }

        [Test]
        public void Creates_New()
        {
            var diff = Config.Compare();
            Config.Ensure(diff);

            datatypeServiceMock
                .Verify(s => s.SaveDataTypeAndPreValues(Match.Create<IDataTypeDefinition>(VerifyDefinition), Match.Create<IDictionary<string, PreValue>>(VerifyPrevalues), 0));
        }

        [Test]
        public void For_Existing_Creates_Missing_Prevalues()
        {
            var extDef = StubDataType(1, "A datatype");
            Mock.Get(extDef).Setup(d => d.PropertyEditorAlias).Returns("nice.editor");
            datatypeServiceMock.Setup(s => s.GetPreValuesCollectionByDataTypeId(1))
                .Returns(new PreValueCollection(new Dictionary<string, PreValue> {{"x", new PreValue(2, "y") }}));

            Config.Ensure(Config.Compare());

            datatypeServiceMock.Verify(s => s.SavePreValues(extDef, Match.Create<IDictionary<string, PreValue>>(VerifyMissingPrevalueUpdate)));
        }

        private bool VerifyMissingPrevalueUpdate(IDictionary<string, PreValue> obj)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(obj));
            return true;
        }

        private bool VerifyPrevalues(IDictionary<string, PreValue> obj)
        {
            Assert.That(obj["x"].Value, Is.EqualTo("y"));
            Assert.That(obj["a"].Value, Is.EqualTo("b"));
            return true;
        }

        private static bool VerifyDefinition(IDataTypeDefinition def)
        {
            Assert.That(def, Has
                .Property("Name").EqualTo("A datatype").And
                .Property("PropertyEditorAlias").EqualTo("nice.editor"));
            return true;
        }
    }
}
