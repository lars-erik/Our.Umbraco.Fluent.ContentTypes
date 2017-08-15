using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Ensuring_DataTypes : ComparisonTestBase
    {
        [Test]
        public void Creates_New()
        {
            Config.DataType("A datatype")
                .PropertyEditor("nice.editor")
                .Prevalue("x", "y")
                .Prevalue("a", "b");

            var diff = Config.Compare();
            if (diff.Safe)
                Config.Ensure(diff);

            Mock.Get(Support.ServiceContext.DataTypeService)
                .Verify(s => s.Save(Match.Create<IDataTypeDefinition>(VerifyDefinition), 0));
        }

        private static bool VerifyDefinition(IDataTypeDefinition def)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(def));
            return true;
        }
    }
}
