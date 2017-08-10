using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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

        protected void StubContentType(string contentTypeAlias, IContentType contentType)
        {
            ContentTypeServiceMock.Setup(t => t.GetContentType(contentTypeAlias)).Returns(contentType);
        }
    }

    [TestFixture]
    public class When_Comparing_Document_Type : ComparisonTestBase
    {
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



        private void When_Umbraco_Has_Document_Type(IContentType contentType, Constraint constraint)
        {
            var alias = "contentType";
            StubContentType(alias, contentType);
            Config.ContentType(alias);
            var diffgram = Config.Compare();
            Assert.That(diffgram.DocumentTypes[alias], constraint);
        }

        public void SampleUsage()
        {
            var config = new FluentContentTypeConfiguration(Support.ServiceContext);

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

    [TestFixture]
    public class When_Comparing_Tab : ComparisonTestBase
    {
        private IContentType contentType;

        [SetUp]
        public void Setup()
        {
            contentType = Mock.Of<IContentType>();
            StubContentType("contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());

            Config.ContentType("contentType")
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

    [TestFixture]
    public class When_Comparing_Property : ComparisonTestBase
    {
        private IContentType contentType;

        [SetUp]
        public void Setup()
        {
            contentType = Mock.Of<IContentType>();
            StubContentType("contentType", contentType);
            contentType.PropertyGroups = new PropertyGroupCollection(new List<PropertyGroup>());

            Config.ContentType("contentType")
                .Tab("tab")
                    .Property("richtext");

        }

        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            var tab = new PropertyGroup() { Name = "tab" };
            contentType.PropertyGroups.Add(tab);
            tab.PropertyTypes.Add(new PropertyType("editor.alias", DataTypeDatabaseType.Ntext) { Alias = "richtext" });

            var diffgram = Config.Compare();

            Assert.That(diffgram.DocumentTypes["contentType"].Tabs["tab"].Properties["richtext"], Has.Property("IsNew").False);
        }

        [Test]
        public void For_Unknown_Then_Is_New()
        {
            var diffgram = Config.Compare();

            Assert.That(diffgram.DocumentTypes["contentType"].Tabs["tab"].Properties["richtext"], Has.Property("IsNew").True);
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

    public class Diffgram
    {
        private readonly FluentContentTypeConfiguration configuration;
        private readonly ServiceContext serviceContext;
        private readonly Dictionary<string, DocumentTypeDiffgram> docTypes;
        private IContentTypeService contentTypeService;

        public Diffgram(FluentContentTypeConfiguration configuration, ServiceContext serviceContext)
        {
            this.configuration = configuration;
            this.serviceContext = serviceContext;
            contentTypeService = serviceContext.ContentTypeService;

            docTypes = new Dictionary<string, DocumentTypeDiffgram>();
        }

        public bool Safe { get; set; }

        public Dictionary<string, DocumentTypeDiffgram> DocumentTypes => docTypes;

        public void Compare()
        {
            foreach (var docType in configuration.DocumentTypes)
            {
                var docTypeDiff = AddDocumentType(docType.Value);
                docTypeDiff.Compare();


            }

        }

        private DocumentTypeDiffgram AddDocumentType(DocumentTypeConfiguration docTypeConfig)
        {
            var docTypeDiffgram = new DocumentTypeDiffgram(docTypeConfig, contentTypeService);
            docTypes.Add(docTypeConfig.Alias, docTypeDiffgram);
            return docTypeDiffgram;
        }
    }

    public class DocumentTypeDiffgram
    {
        private readonly IContentTypeService contentTypeService;
        private readonly DocumentTypeConfiguration configuration;
        public string Alias => configuration.Alias;
        public bool IsNew { get; set; }
        public Dictionary<string, TabDiffgram> Tabs { get; }

        public DocumentTypeDiffgram(DocumentTypeConfiguration configuration, IContentTypeService contentTypeService)
        {
            this.contentTypeService = contentTypeService;
            this.configuration = configuration;
            Tabs = new Dictionary<string, TabDiffgram>();
        }

        public void Compare()
        {
            var umbContentType = contentTypeService.GetContentType(configuration.Alias);

            IsNew = umbContentType == null;

            foreach (var tab in configuration.Tabs.Values)
            {
                var tabDiff = new TabDiffgram(tab, umbContentType.PropertyGroups);
                Tabs.Add(tabDiff.Name, tabDiff);
                tabDiff.Compare();
            }
        }
    }

    public class TabDiffgram
    {
        private readonly TabConfiguration config;
        private readonly PropertyGroupCollection propertyGroups;
        public string Name => config.Name;
        public Dictionary<string, PropertyTypeDiffgram> Properties { get; }

        public bool IsNew { get; private set; }

        public TabDiffgram(TabConfiguration config, PropertyGroupCollection propertyGroups)
        {
            this.config = config;
            this.propertyGroups = propertyGroups;
            Properties = new Dictionary<string, PropertyTypeDiffgram>();
        }

        public void Compare()
        {
            var group = propertyGroups.FirstOrDefault(g => g.Name == config.Name);

            IsNew = group == null;

            foreach (var property in config.Properties.Values)
            {
                var propDiff = new PropertyTypeDiffgram(property, group?.PropertyTypes ?? new PropertyTypeCollection(new PropertyType[0]));
                Properties.Add(property.Alias, propDiff);
                propDiff.Compare();
            }
        }
    }

    public class PropertyTypeDiffgram
    {
        private readonly PropertyConfiguration config;
        private readonly PropertyTypeCollection propertyCollection;

        public bool IsNew { get; private set; }

        public PropertyTypeDiffgram(PropertyConfiguration config, PropertyTypeCollection propertyCollection)
        {
            this.config = config;
            this.propertyCollection = propertyCollection;
        }

        public void Compare()
        {
            var existing = propertyCollection.SingleOrDefault(p => p.Alias == config.Alias);

            IsNew = existing == null;

            if (!IsNew)
            {
                // ...
            }
        }
    }

    public class DocumentTypeConfiguration
    {
        public string Alias { get; }
        public Dictionary<string, TabConfiguration> Tabs { get; private set; }

        public DocumentTypeConfiguration(FluentContentTypeConfiguration parent, string @alias)
        {
            Alias = alias;
            Tabs = new Dictionary<string, TabConfiguration>();
        }

        public TabConfiguration Tab(string tabName)
        {
            var tab = new TabConfiguration(this, tabName);
            Tabs.Add(tab.Name, tab);
            return tab;
        }
    }

    public class TabConfiguration
    {
        public TabConfiguration(DocumentTypeConfiguration parent, string tabName)
        {
            Properties = new Dictionary<string, PropertyConfiguration>();
            Name = tabName;
        }

        public Dictionary<string, PropertyConfiguration> Properties { get; }
        public string Name { get; private set; }

        public PropertyConfiguration Property(string alias)
        {
            var property = new PropertyConfiguration(this, alias);
            Properties.Add(property.Alias, property);
            return property;
        }
    }

    public class PropertyConfiguration
    {
        private readonly TabConfiguration parent;

        public PropertyConfiguration(TabConfiguration parent, string @alias)
        {
            this.parent = parent;
            Alias = @alias;
        }

        public string Alias { get; }

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
