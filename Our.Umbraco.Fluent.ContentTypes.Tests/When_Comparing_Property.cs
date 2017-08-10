using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Comparing_Property : ComparisonTestBase
    {
        private IContentType contentType;
        private PropertyGroup tab;
        private PropertyConfigurator richTextConfig;
        private IDataTypeDefinition dataTypeDefinition;

        [SetUp]
        public void Setup()
        {
            contentType = Mock.Of<IContentType>();
            StubContentType("contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());
            tab = new PropertyGroup() { Name = "tab" };
            contentType.PropertyGroups.Add(tab);

            dataTypeDefinition = StubDataType(5, "richtext");

            AddRichTextProperty();

            richTextConfig = Config
                .ContentType("contentType")
                    .Tab("tab")
                        .Property("richtext")
                            .DisplayName("Rich text")
                            .Description("Write rich content here")
                            .DataType("richtext");
        }

        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsNew").False);
        }

        [Test]
        public void For_Equal_Existing_Then_Is_Not_Unsafe()
        {
            var propertyDiff  = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").False);
        }

        [Test]
        public void For_Unknown_Then_Is_New()
        {
            tab.PropertyTypes.Clear();

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsNew").True);
        }

        [Test]
        public void With_Different_Name_Then_Is_Unsafe()
        {
            richTextConfig.DisplayName("Fancy content");

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").True);
        }

        [Test]
        public void With_Different_Description_Then_Is_Unsafe()
        {
            richTextConfig.Description("Another description");

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").True);
        }

        [Test]
        public void Then_Keeps_List_Of_Comparisons()
        {
            richTextConfig
                .DisplayName("New Name")
                .Description("New description");

            var diff = RichTextDiffgram();

            Assert.That(
                diff.Comparisons,
                Has.Exactly(1).With.Property("Key").EqualTo("Name").And.Property("Result").EqualTo(ComparisonResult.Modified) &
                Has.Exactly(1).With.Property("Key").EqualTo("Description").And.Property("Result").EqualTo(ComparisonResult.Modified) &
                Has.Exactly(1).With.Property("Key").EqualTo("DataType").And.Property("Result").EqualTo(ComparisonResult.Unchanged)
            );
        }

        [Test]
        public void With_Invalid_DataType_Is_Unsafe()
        {
            richTextConfig.DataType("NotTiny");

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").True);
        }

        [Test]
        public void With_Different_DataType_Is_Unsafe()
        {
            StubDataType(6, "NotTiny");
            richTextConfig.DataType("NotTiny");

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").True);
        }

        private void AddRichTextProperty()
        {
            tab.PropertyTypes.Add(
                new PropertyType(dataTypeDefinition)
                {
                    DataTypeDefinitionId = 5,
                    Alias = "richtext",
                    Name = "Rich text",
                    Description = "Write rich content here"
                });
        }

        private PropertyTypeDiffgram RichTextDiffgram()
        {
            var diffgram = Config.Compare();
            var propertyTypeDiffgram = RichTextDiffgram(diffgram);
            return propertyTypeDiffgram;
        }

        private static PropertyTypeDiffgram RichTextDiffgram(Diffgram diffgram)
        {
            return TabDiffgram(diffgram).Properties["richtext"];
        }

        private static TabDiffgram TabDiffgram(Diffgram diffgram)
        {
            return diffgram.DocumentTypes["contentType"].Tabs["tab"];
        }
    }

    public enum ComparisonResult
    {
        Modified,
        Unchanged
    }
}