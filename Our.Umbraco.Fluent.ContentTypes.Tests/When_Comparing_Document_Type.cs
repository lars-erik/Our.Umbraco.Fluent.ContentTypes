using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Our.Umbraco.Fluent.ContentTypes.Tests.Support;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Comparing_Document_Type
    {
        private UmbracoSupport support;
        private Mock<IContentTypeService> contentTypeServiceMock;
        private FluentContentTypeConfiguration config;

        [SetUp]
        public void Setup()
        {
            support = new UmbracoSupport();
            support.SetupUmbraco();
            contentTypeServiceMock = Mock.Get(support.ServiceContext.ContentTypeService);
            config = new FluentContentTypeConfiguration(support.ServiceContext);
        }

        [TearDown]
        public void TearDown()
        {
            support.DisposeUmbraco();
        }

        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            When_Umbraco_Has_Document_Type(Mock.Of<IContentType>(), Has.Property("IsNew").False);
        }

        [Test]
        public void For_Unknown_Then_Is_New()
        {
            When_Umbraco_Has_Document_Type(null, Has.Property("IsNew").True);
        }

        [Test]
        public void Then_Determines_If_Property_Exists()
        {
            var contentType = Mock.Of<IContentType>();
            StubContentType("contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());
            var tab = new PropertyGroup() {Name = "tab"};
            contentType.PropertyGroups.Add(tab);
            tab.PropertyTypes.Add(new PropertyType("editor.alias", DataTypeDatabaseType.Ntext) {Alias="richtext"});

            Assert.Inconclusive("Barely checked that the above can be done in unit test");
        }

        private void When_Umbraco_Has_Document_Type(IContentType contentType, Constraint constraint)
        {
            var alias = "contentType";
            StubContentType(alias, contentType);
            config.ContentType(alias);
            var diffgram = config.Compare();
            Assert.That(diffgram.DocumentTypes[alias], constraint);
        }

        private void StubContentType(string contentTypeAlias, IContentType contentType)
        {
            contentTypeServiceMock.Setup(t => t.GetContentType(contentTypeAlias)).Returns(contentType);
        }

        public void SampleUsage()
        {
            var config = new FluentContentTypeConfiguration(support.ServiceContext);

            config.DataType("richtext")
                .PropertyEditor("Umbraco.TinyMCEv3")
                .DataType(DataTypeDatabaseType.Ntext);

            config.ContentType("contentType")
                .Tab("Content")
                    .Property("text")
                        .DisplayName("The text")
                        .Description("#TextDescription")
                        .DataType("richtext")
                    .Property("Number")
                        .DataType("number");

            var diffgram = config.Compare();

            if (diffgram.Safe)
                config.Ensure(diffgram);
        }
    }

    public class FluentContentTypeConfiguration
    {
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeConfiguration> documentTypes;

        public FluentContentTypeConfiguration(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            documentTypes = new Dictionary<string, DocumentTypeConfiguration>();
        }

        public Dictionary<string, DocumentTypeConfiguration> DocumentTypes => documentTypes;

        public DataTypeConfiguration DataType(string name)
        {
            var dataTypeConfiguration = new DataTypeConfiguration(this, name);
            return dataTypeConfiguration;
        }

        public DocumentTypeConfiguration ContentType(string alias)
        {
            var documentTypeConfiguration = new DocumentTypeConfiguration(this, alias);
            documentTypes.Add(alias, documentTypeConfiguration);
            return documentTypeConfiguration;
        }

        public Diffgram Compare()
        {
            var diffgram = new Diffgram(this, serviceContext);
            diffgram.Compare();
            return diffgram;
        }

        public void Ensure(Diffgram diffgram)
        {
        }
    }

    public class DocumentTypeDiffgram
    {
        public string Alias { get; }
        public bool IsNew { get; set; }

        public DocumentTypeDiffgram(string @alias)
        {
            Alias = alias;
        }
    }

    public class Diffgram
    {
        private readonly FluentContentTypeConfiguration configuration;
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeDiffgram> docTypes;

        public Diffgram(FluentContentTypeConfiguration configuration, ServiceContext serviceContext)
        {
            this.configuration = configuration;
            this.serviceContext = serviceContext;

            docTypes = new Dictionary<string, DocumentTypeDiffgram>();
        }

        public bool Safe { get; set; }

        public Dictionary<string, DocumentTypeDiffgram> DocumentTypes => docTypes;

        public void Compare()
        {
            foreach (var docType in configuration.DocumentTypes)
            {
                var contentTypeService = serviceContext.ContentTypeService;
                var umbContentType = contentTypeService.GetContentType(docType.Key);
                var docTypeDiff = AddDocumentType(docType.Key);
                docTypeDiff.IsNew = umbContentType == null;
            }

        }

        private DocumentTypeDiffgram AddDocumentType(string alias)
        {
            var docTypeDiffgram = new DocumentTypeDiffgram(alias);
            docTypes.Add(alias, docTypeDiffgram);
            return docTypeDiffgram;
        }
    }

    public class DocumentTypeConfiguration
    {
        public DocumentTypeConfiguration(FluentContentTypeConfiguration parent, string @alias)
        {
        }

        public TabConfiguration Tab(string tabName)
        {
            return new TabConfiguration(this, tabName);
        }
    }

    public class TabConfiguration
    {
        public TabConfiguration(DocumentTypeConfiguration parent, string tabName)
        {
        }

        public PropertyConfiguration Property(string alias)
        {
            return new PropertyConfiguration(this, alias);
        }
    }

    public class PropertyConfiguration
    {
        private readonly TabConfiguration parent;

        public PropertyConfiguration(TabConfiguration parent, string @alias)
        {
            this.parent = parent;
        }

        public PropertyConfiguration DisplayName(string displayName)
        {
            return this;
        }

        public PropertyConfiguration Description(string description)
        {
            return this;
        }

        public PropertyConfiguration DataType(string description)
        {
            return this;
        }

        public PropertyConfiguration Property(string alias)
        {
            return parent.Property(alias);
        }
    }

    public class DataTypeConfiguration
    {
        public DataTypeConfiguration(FluentContentTypeConfiguration parent, string name)
        {
        }

        public DataTypeConfiguration PropertyEditor(string propertyEditorAlias)
        {
            return this;
        }

        public DataTypeConfiguration DataType(DataTypeDatabaseType type)
        {
            return this;
        }
    }
}
