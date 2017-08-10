using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
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
}
