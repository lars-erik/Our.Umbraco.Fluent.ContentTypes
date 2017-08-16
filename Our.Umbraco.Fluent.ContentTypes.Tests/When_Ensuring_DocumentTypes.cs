using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    [UseReporter(typeof(VisualStudioReporter))]
    public class When_Ensuring_DocumentTypes : ComparisonTestBase
    {
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

            var diff = Config.Compare();

            var orderedTypes = OrderByDependencies(diff, diff.DocumentTypes.Values);

            Console.WriteLine(orderedTypes.Select(t => t.Key).ToJson());
            Assert.Inconclusive();

            Approvals.VerifyJson(orderedTypes.Select(t => t.Key).ToJson());
        }

        private IEnumerable<DocumentTypeDiffgram> OrderByDependencies(Diffgram diffgram, IEnumerable<DocumentTypeDiffgram> documentTypesValues)
        {
            var comparer = new DependencyComparer();
            return documentTypesValues.OrderBy(x => x, comparer);
        }

        public class DependencyComparer : IComparer<DocumentTypeDiffgram>
        {
            public DependencyComparer()
            {
            }

            public int Compare(DocumentTypeDiffgram x, DocumentTypeDiffgram y)
            {
                if (x == null || y == null) throw new Exception("Can't sort null diffgrams");
                var xDependsOnY = DependsOn(x, y);
                var yDependsOnX = DependsOn(y, x);

                if (xDependsOnY && yDependsOnX)
                    throw new Exception("This looks like a circular reference. :(");

                var retVal = 0;

                if (xDependsOnY) retVal = 1;
                if (yDependsOnX) retVal = -1;

                Console.WriteLine($"{x.Key} depends on {y.Key}: {retVal}");

                return retVal;
            }

            private bool DependsOn(DocumentTypeDiffgram x, DocumentTypeDiffgram y)
            {
                var dependencies = x.GetDependencies();
                Console.WriteLine($"{x.Key}: {String.Join(", ", dependencies)}");
                var dependsOn = dependencies.Contains(y.Key);

                return dependsOn;
            }
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
            Approvals.VerifyJson(obj.ToJson());
            return true;
        }
    }
}
