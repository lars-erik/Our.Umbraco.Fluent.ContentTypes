using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Ensuring_Templates : ComparisonTestBase
    {
        [Test]
        public void Creates_New()
        {
            Config.Template("niceTemplate")
                .Name("A nice template");

            Config.Ensure(Config.Compare());

            Mock.Get(Support.ServiceContext.FileService)
                .Verify(s => s.SaveTemplate(
                    Match.Create<ITemplate>(VerifyTemplate),
                    0
                ));
        }

        private static bool VerifyTemplate(ITemplate t)
        {
            Assert.That(t, Has.Property("Alias").EqualTo("niceTemplate").And.Property("Name").EqualTo("A nice template"));
            return true;
        }
    }
}
