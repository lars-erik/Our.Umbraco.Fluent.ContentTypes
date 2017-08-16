using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Comparing_Tab : ComparisonTestBase
    {
        private IContentType contentType;

        [SetUp]
        public void Setup()
        {
            contentType = Mock.Of<IContentType>();
            StubContentType(1, "contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());

            Config.DocumentType("contentType")
                .Tab("tab");
        }

        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            contentType.PropertyGroups.Add(new PropertyGroup() {Name = "tab"});

            var diffgram = Config.Compare();

            Assert.That(diffgram.DocumentTypes["contentType"].Tabs["tab"], Has.Property("IsNew").False);
        }

        [Test]
        public void For_Unknown_Then_Is_New()
        {
            var diffgram = Config.Compare();

            Assert.That(diffgram.DocumentTypes["contentType"].Tabs["tab"], Has.Property("IsNew").True);
        }
    }
}