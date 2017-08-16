using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using AutoMapper;
using ClientDependency.Core.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.BootManagers;
using Umbraco.Tests.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    [UseReporter(typeof(VisualStudioReporter))]
    public partial class When_Ensuring_DocumentTypes : ComparisonTestBase
    {
        [SetUp]
        public void SetupMapping()
        {
            //create a fake property editor resolver to return fake property editors

            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;

            var _propertyEditorResolver = new Mock<PropertyEditorResolver>(

                //ctor args

                Mock.Of<IServiceProvider>(), Mock.Of<ILogger>(), typeListProducerList, CacheHelper.CreateDisabledCacheHelper().RuntimeCache);



            var ctor = Type.GetType("Umbraco.Web.Models.Mapping.ContentTypeModelMapper, umbraco")
                .GetConstructor(new[] { typeof(Lazy<PropertyEditorResolver>) });
            var entityctor = Type.GetType("Umbraco.Web.Models.Mapping.EntityModelMapper, umbraco")
                .GetConstructor(new Type[0]);
            Mapper.Initialize(configuration =>

            {
                //initialize our content type mapper
                var mapper = (MapperConfiguration)ctor.Invoke(new[]{new Lazy<PropertyEditorResolver>(() => _propertyEditorResolver.Object) });
                mapper.ConfigureMappings(configuration, Support.UmbracoContext.Application);

                var entityMapper = (MapperConfiguration)entityctor.Invoke(new object[0]);
                entityMapper.ConfigureMappings(configuration, Support.UmbracoContext.Application);
            });
        }

        [Test]
        public void Orders_Dependent_Types_Later_Than_Their_Dependencies()
        {
            /*
            Setup

            Alias    Parent    AllowedChildren    Compositions      Dependencies           Indirect                      All
            d-1-1    d-1;                         cmp-1-1; cmp-2;   d-1; cmp-1-1; cmp-2;   cmp-1;                        d-1; cmp-1-1; cmp-2; cmp-1; 
            cmp-1-1                               cmp-1;            cmp-1;                                               cmp-1; 
            cmp-2                        
            d-2                d-1; d-1-1;                          d-1; d-1-1;            d-1; cmp-1-1; cmp-2; cmp-1;   d-1; d-1-1; d-1; cmp-1-1; cmp-2; cmp-1; 
            cmp-1                                                                                  
            d-1-2    d-1;                                           d-1;                   cmp-1;                        d-1; cmp-1; 
            d-1                                   cmp-1;            cmp-1;                                               cmp-1; 

            Expectation
            cmp-1
            cmp-2
            cmp-1-1
            d-1
            d-1-1
            d-1-2
            d-2

            */


            Config.DocumentType("d-1-1")
                .Parent("d-1")
                .Compositions("cmp-1-1", "cmp-2");

            Config.DocumentType("cmp-1-1")
                .Compositions("cmp-1");

            Config.DocumentType("cmp-2");

            Config.DocumentType("d-2")
                .AllowedAsRoot(true)
                .AllowedChildren("d-1", "d-1-1");

            Config.DocumentType("cmp-1");

            Config.DocumentType("d-1-2")
                .Parent("d-1");

            Config.DocumentType("d-1")
                .Compositions("cmp-1");
            //.AllowedChildren("d-1-1"); // dammit! - what about circular references? (parent/allowedchildren) - rare UC?
            // See prevents circular reference test.

            var diff = Config.Compare();

            var orderedTypes = DependencyComparer.OrderByDependencies(diff.DocumentTypes.Values);
            var aliases = orderedTypes.Select(t => t.Key);

            Approvals.VerifyJson(aliases.ToJson(Formatting.None));
        }

        [Test]
        public void Prevents_Circular_References()
        {
            Config.DocumentType("a")
                .Parent("b");

            Config.DocumentType("b")
                .AllowedChildren("a");

            Assert.That(() =>
                Config.Ensure(Config.Compare()),
                Throws.Exception.InnerException.With
                    .Message.EqualTo("Seems like we've hit a rift in time-space continuum. (Circular reference)")
            );
        }

        [Test]
        public void Creates_New()
        {
            StubTemplate("template");

            StubContentType(1, "parent");
            StubContentType(2, "child");
            StubContentType(3, "mixin");

            Config.DocumentType("docType")
                .Name("A nice doctype")
                .Description("A fine description")
                .Icon("icon-folder")
                .AllowedAsRoot(true)
                .Parent("parent")
                .AllowedTemplates("template")
                .AllowedChildren("child")
                .Compositions("mixin");

            Config.Ensure(Config.Compare());

            Mock.Get(Support.ServiceContext.ContentTypeService)
                .Verify(s => s.Save(Match.Create<IContentType>(VerifyContentType), 0));
        }

        private bool VerifyContentType(IContentType obj)
        {
            var jsonable = Mapper.Map<DocumentTypeDisplay>(obj);
            Approvals.VerifyJson(jsonable.ToJson());
            return true;
        }
    }
}
