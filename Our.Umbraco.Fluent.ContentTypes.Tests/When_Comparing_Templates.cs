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
    public class When_Comparing_Templates : ComparisonTestBase
    {
        private const string ExpectedAlias = "fancy";
        private ITemplate template;

        [SetUp]
        public void Setup()
        {
            Config.Template(ExpectedAlias)
                .Name("Fancy template");

            template = StubTemplate(ExpectedAlias, new Guid("3F459DA6-CE3C-458C-AF0E-66104800F6EA"));
        }

        [Test]
        public void Then_Existing_Equal_Is_Safe()
        {
            Mock.Get(template).Setup(t => t.Name).Returns("Fancy template");
            StubTemplate(template);

            Config.Compare().Verify();
        }

        [Test]
        public void Then_Existing_Modified_Is_Unsafe()
        {
            Mock.Get(template).Setup(t => t.Name).Returns("Real fancy template");
            StubTemplate(template);

            Config.Compare().Verify();
        }

        [Test]
        public void Then_New_Is_Safe()
        {
            Mock.Get(Support.ServiceContext.FileService).Reset();
            Mock.Get(Support.ServiceContext.FileService).Setup(s => s.GetTemplate(ExpectedAlias)).Returns<ITemplate>(null);

            Config.Compare().Verify();
        }
    }
}
