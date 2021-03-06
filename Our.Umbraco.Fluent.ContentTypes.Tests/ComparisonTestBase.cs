using System;
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
                contentType.Path = "-1," + id;
            }
            ContentTypeServiceMock.Setup(t => t.GetContentType(id)).Returns(contentType);
            ContentTypeServiceMock.Setup(t => t.GetContentType(contentTypeAlias)).Returns(contentType);
            return contentType;
        }

        protected IDataTypeDefinition StubDataType(int id, string dataTypeName)
        {
            var definition = Mock.Of<IDataTypeDefinition>();
            definition.Id = id;
            definition.Name = dataTypeName;
            Mock.Get(Support.ServiceContext.DataTypeService).Setup(s => s.GetDataTypeDefinitionByName(dataTypeName)).Returns(definition);
            return definition;
        }

        protected ITemplate StubTemplate(string alias, Guid? guid = null)
        {
            guid = guid ?? Guid.NewGuid();
            var newTemplate = Mock.Of<ITemplate>();
            Mock.Get(newTemplate).Setup(t => t.Alias).Returns(alias);
            Mock.Get(newTemplate).Setup(t => t.Key).Returns(guid.Value);
            StubTemplate(newTemplate);
            return newTemplate;
        }

        protected void StubTemplate(ITemplate value)
        {
            if (value != null)
            { 
                Mock.Get(Support.ServiceContext.FileService).Setup(s => s.GetTemplate(value.Alias)).Returns(value);
            }
        }
    }
}