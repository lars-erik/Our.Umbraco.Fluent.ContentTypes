using Moq;
using NUnit.Framework;
using Our.Umbraco.Fluent.ContentTypes.Tests.Support;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class ComparisonTestBase
    {
        protected UmbracoSupport Support;
        protected Mock<IContentTypeService> ContentTypeServiceMock;
        protected FluentContentTypeConfiguration Config;

        [SetUp]
        protected void SetupBase()
        {
            Support = new UmbracoSupport();
            Support.SetupUmbraco();

            ContentTypeServiceMock = Mock.Get(Support.ServiceContext.ContentTypeService);
            Config = new FluentContentTypeConfiguration(Support.ServiceContext);
        }

        [TearDown]
        protected void TearDownBase()
        {
            Support.DisposeUmbraco();
        }

        protected IContentType StubContentType(int id, string contentTypeAlias)
        {
            var contentType = Mock.Of<IContentType>();
            StubContentType(id, contentTypeAlias, contentType);
            return contentType;
        }

        protected IContentType StubContentType(int id, string contentTypeAlias, IContentType contentType)
        {
            if (contentType != null)
            {
                contentType.Id = id;
                contentType.Alias = contentTypeAlias;
            }
            ContentTypeServiceMock.Setup(t => t.GetContentType(id)).Returns(contentType);
            ContentTypeServiceMock.Setup(t => t.GetContentType(contentTypeAlias)).Returns(contentType);
            return contentType;
        }

        protected IDataTypeDefinition StubDataType(int id, string dataTypeName)
        {
            var definition = Mock.Of<IDataTypeDefinition>();
            definition.Id = id;
            definition.Name = "RichText";
            Mock.Get(Support.ServiceContext.DataTypeService).Setup(s => s.GetDataTypeDefinitionByName(dataTypeName)).Returns(definition);
            return definition;
        }
    }
}