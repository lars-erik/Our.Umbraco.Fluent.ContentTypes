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

        [SetUp]
        public void Setup()
        {
            contentType = Mock.Of<IContentType>();
            StubContentType("contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());
            tab = new PropertyGroup() { Name = "tab" };
            contentType.PropertyGroups.Add(tab);
            AddRichTextProperty();

            richTextConfig = Config
                .ContentType("contentType")
                    .Tab("tab")
                        .Property("richtext")
                            .DisplayName("Rich text")
                            .Description("Write rich content here")
                            .DataType("Umbraco.TinyMCEv3");
        }

        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            var propertyDiff  = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsNew").False);
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
        public void With_Different_Data_Type_Alias_Then_Is_Unsafe()
        {
            richTextConfig.DataType("NotTiny");

            var propertyDiff = RichTextDiffgram();

            Assert.That(propertyDiff, Has.Property("IsUnsafe").True);
        }

        private void AddRichTextProperty()
        {
            tab.PropertyTypes.Add(new PropertyType("Umbraco.TinyMCEv3", DataTypeDatabaseType.Ntext) {Alias = "richtext", Name="Rich text", Description = "Write rich content here" });
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
}